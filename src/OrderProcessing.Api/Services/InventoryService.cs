using OrderProcessing.Api.Data;
using OrderProcessing.Api.Models;
using Microsoft.EntityFrameworkCore;
using OrderProcessing.Api.Interfaces;
using static OrderProcessing.Api.Dtos.InventoryDto;

namespace OrderProcessing.Api.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService (AppDbContext appDbContext, ILogger<InventoryService> logger)
        {
            _appDbContext = appDbContext;
            _logger = logger;
        }

        public async Task<InventoryItem?> GetAvailableProductsAsync(string productId)
        {
            var availableStock = await _appDbContext.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == productId);
            return availableStock;
        }

        //will come back to this later
        public async Task<InventoryItem?> ReserveStockAsync(string product, int quantity)
        {
            var inventoryItem = await GetAvailableProductsAsync(product);

            if (inventoryItem.AvailableQuantity < quantity)
            {
                _logger.LogWarning($"Cannot reserve {quantity} units for product ID: {product}. Available quantity: {inventoryItem.AvailableQuantity}");
                return null;
            }

            await _appDbContext.SaveChangesAsync();

            return inventoryItem;
        }
    }
}
