using OrderProcessing.Api.Models;

namespace OrderProcessing.Api.Interfaces
{
    public interface IInventoryService
    {
        Task<InventoryItem?> GetAvailableProductsAsync(string productId);
        Task<InventoryItem?> ReserveStockAsync(string productId, int quantity);
    }
}
