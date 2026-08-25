using OrderProcessing.Api.Dtos;
using OrderProcessing.Api.Models;

namespace OrderProcessing.Api.Interfaces
{
    public interface IOrderService
    {
        Task<(Order? Order, string? Error)> CreateOrderAsync(CreateOrderRequest request);
        Task<Order?> GetOrderByIdAsync(Guid id);
        Task<IEnumerable<Order>> GetOrdersAsync(int page, int pageSize);
        Task<(Order? Order, string? Error)> UpdateOrderStatusAsync(Guid id, OrderStatus status);
    }
}