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

        public async Task<InventoryItem?> ReserveStockAsync(string productId, int quantity, InventoryItem inventoryItem)
        {
            inventoryItem.AvailableQuantity -= quantity; // removed requsted quantity from available quantity
            inventoryItem.ReservedQuantity += quantity; // add new quantity to reserved quantity
            await _appDbContext.SaveChangesAsync();
            return inventoryItem;
        }

        public async Task<InventoryItem?> ReleaseStockAsync(string productId, int quantity, InventoryItem inventoryItem)
        {
            inventoryItem.ReservedQuantity -= quantity; // remove requested stock from reserved quantity
            inventoryItem.AvailableQuantity += quantity; // add requested stock back to the available quantity
            await _appDbContext.SaveChangesAsync();

            return inventoryItem;
        }
    }
}
