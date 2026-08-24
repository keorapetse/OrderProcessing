using Microsoft.EntityFrameworkCore;
using OrderProcessing.Api.Data;
using OrderProcessing.Api.Dtos;
using OrderProcessing.Api.Interfaces;
using OrderProcessing.Api.Models;
using System.Net;
using static OrderProcessing.Api.Dtos.InventoryDto;

namespace OrderProcessing.Api.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<OrderService> _logger;
        private readonly HttpClient _httpClient;

        public OrderService(
            AppDbContext db,
            ILogger<OrderService> logger,
            HttpClient httpClient)
        {
            _db = db;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<Order?> CreateOrderAsync(CreateOrderRequest request)
        {
            // Calculate the total amount from the submitted items.
            var totalAmount = request.Items.Sum(item => item.Quantity * item.UnitPrice);

            // Create the database entity from the request DTO.
            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                TotalAmount = totalAmount,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Items = request.Items.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                })
                .ToList()
            };

            // Save the order as Pending before starting the processing flow.
            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            var reservedItems = new List<(string ProductId, int Quantity)>();

            try
            {
                // 1.Check inventory availability for every item.
                foreach (var item in order.Items)
                {
                    var inventoryResponse = await _httpClient.GetAsync($"api/inventory/{item.ProductId}");

                    if (inventoryResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        _logger.LogWarning("No inventory found for product {ProductId}.", item.ProductId);
                        await CancelOrderAsync(order);
                        return null;
                    }

                    if (!inventoryResponse.IsSuccessStatusCode)
                    {
                        _logger.LogError("Inventory service returned status code {StatusCode} for product {ProductId}.", inventoryResponse.StatusCode, item.ProductId);
                        await CancelOrderAsync(order);

                        return null;
                    }

                    var inventoryItem = await inventoryResponse.Content.ReadFromJsonAsync<InventoryItem>();

                    if (inventoryItem == null || inventoryItem.AvailableQuantity < item.Quantity)
                    {
                        _logger.LogWarning(
                            "Insufficient inventory for product {ProductId}. Requested: {RequestedQuantity}, Available: {AvailableQuantity}.",
                            item.ProductId,
                            item.Quantity,
                            inventoryItem?.AvailableQuantity ?? 0);

                        await CancelOrderAsync(order);
                        return null;
                    }
                }

                // 2. Reserve inventory for every item.
                foreach (var item in order.Items)
                {
                    var stockRequest = new StockChangeRequest
                    {
                        Quantity = item.Quantity
                    };

                    var reserveResponse = await _httpClient.PostAsJsonAsync($"api/inventory/{item.ProductId}/reserve", stockRequest);

                    if (!reserveResponse.IsSuccessStatusCode)
                    {
                        _logger.LogWarning(
                            "Failed to reserve {Quantity} units for product {ProductId}.",
                            item.Quantity,
                            item.ProductId);

                        await ReleaseReservedItemsAsync(reservedItems);
                        await CancelOrderAsync(order);
                        return null;
                    }

                    reservedItems.Add((item.ProductId, item.Quantity));
                }

                // 3. Process payment.
                var paymentRequest = new ProcessPaymentRequest
                {
                    OrderId = order.Id,
                    Amount = order.TotalAmount
                };

                var paymentResponse = await _httpClient.PostAsJsonAsync("api/payments/process", paymentRequest);

                if (!paymentResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Payment processing failed for order {OrderId}.", order.Id);
                    await ReleaseReservedItemsAsync(reservedItems);
                    await CancelOrderAsync(order);
                    return null;
                }

                var paymentTransaction = await paymentResponse.Content.ReadFromJsonAsync<PaymentTransaction>();

                if (paymentTransaction == null || paymentTransaction.Status != PaymentStatus.Completed)
                {
                    _logger.LogWarning("Payment was not completed for order {OrderId}.", order.Id);

                    await ReleaseReservedItemsAsync(reservedItems);
                    await CancelOrderAsync(order);
                    return null;
                }

                //4. Payment succeeded, so confirm the order.
                order.Status = OrderStatus.Confirmed;
                order.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                _logger.LogInformation("Order {OrderId} was successfully created and confirmed.", order.Id);
                return order;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "A service was unavailable while processing order {OrderId}.", order.Id);
                await ReleaseReservedItemsAsync(reservedItems);
                await CancelOrderAsync(order);
                return null;
            }
        }

        public async Task<Order?> GetOrderByIdAsync(Guid id)
        {
            return await _db.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .FirstOrDefaultAsync(
                    o => o.Id == id);
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync(int page, int pageSize)
        {
            return await _db.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Order?> UpdateOrderStatusAsync(Guid id, OrderStatus status)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return null;
            }

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            _logger.LogInformation("Order {OrderId} status updated to {Status}.", id, status);

            return order;
        }

        private async Task ReleaseReservedItemsAsync(List<(string ProductId, int Quantity)> reservedItems)
        {
            foreach (var item in reservedItems)
            {
                try
                {
                    var stockRequest = new StockChangeRequest
                    {
                        Quantity = item.Quantity
                    };

                    var response = await _httpClient.PostAsJsonAsync($"api/inventory/{item.ProductId}/release", stockRequest);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError(
                            "Failed to release {Quantity} units for product {ProductId}. Status code: {StatusCode}",
                            item.Quantity,
                            item.ProductId,
                            response.StatusCode);
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Failed to release inventory for product {ProductId}.", item.ProductId);
                }
            }
        }

        private async Task CancelOrderAsync(Order order)
        {
            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}