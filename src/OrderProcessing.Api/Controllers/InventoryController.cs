using Microsoft.AspNetCore.Mvc;
using OrderProcessing.Api.Interfaces;

namespace OrderProcessing.Api.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetAvailableProducts(string productId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(productId))
                {
                    return BadRequest("Product ID is required.");
                }

                var availableInventory = await _inventoryService.GetAvailableProductsAsync(productId);

                if (availableInventory == null) 
                {
                    return NotFound($"No stock found for product ID: {productId}");
                }

                return Ok(availableInventory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get inventory for {productId}", productId);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("{productId}/reserve")]
        public async Task<IActionResult> ReserveStock(string productId, int quantity)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(productId))
                {
                    return BadRequest("Product ID is required.");
                }

                if (quantity <= 0)
                {
                    return BadRequest("Quantity must be greater than zero.");
                }

                var inventoryItem = await _inventoryService.GetAvailableProductsAsync(productId);

                if (inventoryItem == null)
                {
                    return NotFound($"No stock was found for product ID: {productId}");
                }

                if (quantity > inventoryItem.AvailableQuantity)
                {
                    return BadRequest($"Requested quantity of {quantity} exceeds available quantity of {inventoryItem.AvailableQuantity}.");
                }

                var reservedInventory = await _inventoryService.ReserveStockAsync(productId, quantity, inventoryItem);

                if (reservedInventory == null)
                {
                    return BadRequest("Unable to reserve inventory.");
                }

                return Ok(reservedInventory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to reserve {quantity} for {productId}");
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("{productId}/release")]
        public async Task<IActionResult> ReleaseStock(string productId, int quantity)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(productId))
                {
                    return BadRequest("Product ID is required.");
                }

                if (quantity <= 0)
                {
                    return BadRequest("Quantity must be greater than zero.");
                }

                var inventoryItem = await _inventoryService.GetAvailableProductsAsync(productId);

                if (inventoryItem == null)
                {
                    return NotFound($"No stock was found for product ID: {productId}");
                }

                if (quantity > inventoryItem.ReservedQuantity)
                {
                    return BadRequest($"Requested quantity of {quantity} exceeds the reserved quantity of {inventoryItem.ReservedQuantity}.");
                }

                var releasedStock = await _inventoryService.ReleaseStockAsync(productId, quantity, inventoryItem);

                if (releasedStock == null)
                {
                    return NotFound($"No inventory found or unable to release {quantity} units for product ID: {productId}");
                }

                return Ok(releasedStock);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to release {quantity} for {productId}");
                return StatusCode(StatusCodes.Status500InternalServerError,ex.Message);
            }
        }
    }
}
