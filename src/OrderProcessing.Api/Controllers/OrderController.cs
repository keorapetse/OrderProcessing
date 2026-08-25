using Microsoft.AspNetCore.Mvc;
using OrderProcessing.Api.Dtos;
using OrderProcessing.Api.Interfaces;
using OrderProcessing.Api.Models;

namespace OrderProcessing.Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Order request is required.");
                }

                if (string.IsNullOrWhiteSpace(request.CustomerId))
                {
                    return BadRequest("Customer ID is required.");
                }

                if (request.Items == null || !request.Items.Any())
                {
                    return BadRequest("Order must contain at least one item.");
                }

                var order = await _orderService.CreateOrderAsync(request);

                if (order == null)
                {
                    return BadRequest("Unable to create order.");
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create order for customer {CustomerId}", request?.CustomerId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the order.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("Order ID is required.");
                }

                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null)
                {
                    return NotFound($"Order not found for order ID {id}.");
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get order {OrderId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching the order.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page <= 0)
                {
                    return BadRequest("Page must be greater than zero.");
                }

                if (pageSize <= 0)
                {
                    return BadRequest("Page size must be greater than zero.");
                }

                var orders = await _orderService.GetOrdersAsync(page, pageSize);

                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get orders. Page: {Page}, PageSize: {PageSize}", page, pageSize);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching orders.");
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] OrderStatus status)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("Order ID is required.");
                }

                if (!Enum.IsDefined(typeof(OrderStatus), status))
                {
                    return BadRequest("Invalid order status.");
                }

                var order = await _orderService.UpdateOrderStatusAsync(id, status);

                if (order == null)
                {
                    return NotFound($"Order not found for order ID {id}.");
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update status for order {OrderId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the order status.");
            }
        }
    }
}