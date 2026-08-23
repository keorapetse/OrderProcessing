using Microsoft.AspNetCore.Mvc;
using OrderProcessing.Api.Interfaces;
using static OrderProcessing.Api.Dtos.InventoryDto;

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
        public async Task<IActionResult> GetAvailableProducts(string productId, CancellationToken cancellationToken)
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
                    return NotFound($"No inventory found for product ID: {productId}");
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
        public async Task<IActionResult> ReserveStock([FromBody] StockChangeRequest request, string productId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(productId))
                {
                    return BadRequest("Product ID is required.");
                }

                if (request == null || request.Quantity <= 0)
                {
                    return BadRequest("Quantity must be greater than zero.");
                }

                var inventoryItem = await _inventoryService.ReserveStockAsync(productId, request.Quantity);

                if (inventoryItem == null)
                {
                    return NotFound($"No inventory found or unable to reserve {request.Quantity} units for product ID: {productId}");
                }

                return Ok(inventoryItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to reserve {request.Quantity} for {productId}");
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
