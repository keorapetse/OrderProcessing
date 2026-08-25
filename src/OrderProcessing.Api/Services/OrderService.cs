using Microsoft.EntityFrameworkCore;
using OrderProcessing.Api.Data;
using OrderProcessing.Api.Dtos;
using OrderProcessing.Api.Interfaces;
using OrderProcessing.Api.Models;
using System.Net;

namespace OrderProcessing.Api.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<OrderService> _logger;
        private readonly HttpClient _httpClient;

        public OrderService(AppDbContext db, ILogger<OrderService> logger, HttpClient httpClient)
        {
            _db = db;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<(Order? Order, string? Error)> CreateOrderAsync(CreateOrderRequest request)
        {
            // Calculate the total amount from the submitted items.
            var totalAmount = request.Items.Sum(item => item.Quantity * item.UnitPrice);

            // Validate the total amount early to provide a clear error when the order total is invalid
            if (totalAmount <= 0)
            {
                _logger.LogWarning("Order total is invalid ({TotalAmount}) for customer {CustomerId}.", totalAmount, request.CustomerId);
                return (null, $"Order total must be greater than zero. Calculated total: {totalAmount}.");
            }

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
                // 1.Check if we have stock for requested item
                foreach (var item in order.Items)
                {
                    var inventoryResponse = await _httpClient.GetAsync($"api/inventory/{item.ProductId}");

                    if (inventoryResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        _logger.LogWarning("No stock found for product {ProductId}.", item.ProductId);
                        await CancelOrderAsync(order);
                        return (null, $"No stock found for product {item.ProductId}.");
                    }

                    if (!inventoryResponse.IsSuccessStatusCode)
                    {
                        _logger.LogError("Inventory service returned status code {StatusCode} for product {ProductId}.", inventoryResponse.StatusCode, item.ProductId);
                        await CancelOrderAsync(order);
                        return (null, $"Inventory service returned status code {(int)inventoryResponse.StatusCode} for product {item.ProductId}."
);
                    }

                    var inventoryItem = await inventoryResponse.Content.ReadFromJsonAsync<InventoryItem>();

                    if (inventoryItem == null || inventoryItem.AvailableQuantity < item.Quantity)
                    {
                        _logger.LogWarning(
                            "Insufficient stock for product {ProductId}. Requested: {RequestedQuantity}, Available: {AvailableQuantity}.",
                            item.ProductId,
                            item.Quantity,
                            inventoryItem?.AvailableQuantity ?? 0);

                        await CancelOrderAsync(order);
                        return (null, $"Insufficient stock for product {item.ProductId}. " + $"Requested: {item.Quantity}, Available: {inventoryItem?.AvailableQuantity ?? 0}.");
                    }
                }

                // 2. Reserve inventory for every item.
                foreach (var item in order.Items)
                {
                    var reserveResponse = await _httpClient.PostAsync($"api/inventory/{item.ProductId}/reserve?quantity={item.Quantity}", null);

                    if (!reserveResponse.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Failed to reserve {Quantity} units for product {ProductId}.", item.Quantity, item.ProductId);
                        await ReleaseReservedItemsAsync(reservedItems);
                        await CancelOrderAsync(order);
                        return (null, $"Failed to reserve {item.Quantity} units for product {item.ProductId}. " + $"Inventory service returned status code {(int)reserveResponse.StatusCode}."
                      );
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

                // if payment is unsuccessful, release the reserved stock
                if (!paymentResponse.IsSuccessStatusCode)
                {
                    var paymentErrorBody = await paymentResponse.Content.ReadAsStringAsync();
                    _logger.LogWarning("Payment processing failed for order {OrderId}. Payment service responded with status {StatusCode} and body: {Body}", order.Id, paymentResponse.StatusCode, paymentErrorBody);
                    await ReleaseReservedItemsAsync(reservedItems);
                    await CancelOrderAsync(order);
                    return (null, $"Payment processing failed for order {order.Id}. Payment service response: {paymentErrorBody}");
                }

                var paymentResponseBody = await paymentResponse.Content.ReadAsStringAsync();

                var paymentTransaction = System.Text.Json.JsonSerializer.Deserialize<PaymentTransaction>(paymentResponseBody, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web)
             {
            Converters =
            {
                new System.Text.Json.Serialization.JsonStringEnumConverter(
                    System.Text.Json.JsonNamingPolicy.CamelCase)
            }
        });

                if (paymentTransaction == null || paymentTransaction.Status != PaymentStatus.Completed)
                {
                    _logger.LogWarning("Payment was not completed for order {OrderId}. Response body: {Body}", order.Id, paymentResponseBody);
                    await ReleaseReservedItemsAsync(reservedItems);
                    await CancelOrderAsync(order);
                    return (null, $"Payment was not completed for order {order.Id}. Payment response: {paymentResponseBody}");
                }

                //4. Payment succeeded, so confirm the order.
                order.Status = OrderStatus.Confirmed;
                order.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                _logger.LogInformation("Order {OrderId} was successfully created and confirmed.", order.Id);
                return (order, null);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "A service was unavailable while processing order {OrderId}.", order.Id);
                await ReleaseReservedItemsAsync(reservedItems);
                await CancelOrderAsync(order);
                return (null, ex.Message);
            }
        }

        // return orders that were created using id
        public async Task<Order?> GetOrderByIdAsync(Guid id)
        {
            return await _db.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .FirstOrDefaultAsync(
                    o => o.Id == id);
        }

        // get the list of orders
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

        public async Task<(Order? Order, string? Error)> UpdateOrderStatusAsync(Guid id, OrderStatus status)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return (null, $"Order with ID {id} was not found.");
            }

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            _logger.LogInformation("Order {OrderId} status updated to {Status}.", id, status);

            return (order, null);
        }

        private async Task ReleaseReservedItemsAsync(List<(string ProductId, int Quantity)> reservedItems)
        {
            foreach (var item in reservedItems)
            {
                try
                {
                    var response = await _httpClient.PostAsync($"api/inventory/{item.ProductId}/release?quantity={item.Quantity}", null);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError("Failed to release {Quantity} units for product {ProductId}. Status code: {StatusCode}", item.Quantity, item.ProductId, response.StatusCode);
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