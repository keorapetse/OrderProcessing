using OrderProcessing.Api.Dtos;
using OrderProcessing.Api.Models;

namespace OrderProcessing.Api.Interfaces
{
    public interface IOrderService
    {
        Task<Order?> CreateOrderAsync(CreateOrderRequest request);
        Task<Order?> GetOrderByIdAsync(Guid id);
        Task<IEnumerable<Order>> GetOrdersAsync(int page, int pageSize);
        Task<Order?> UpdateOrderStatusAsync(Guid id, OrderStatus status);
    }
}