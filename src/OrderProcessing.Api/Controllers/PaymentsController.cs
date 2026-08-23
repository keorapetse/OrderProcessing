using Microsoft.AspNetCore.Mvc;
using OrderProcessing.Api.Interfaces;
using OrderProcessing.Api.Dtos;

namespace OrderProcessing.Api.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
        {
            try
            {
                if (request.OrderId == Guid.Empty)
                {
                    return BadRequest("Order ID is required.");
                }

                if (request.Amount <= 0)
                {
                    return BadRequest("Payment amount must be greater than zero");
                }

                var paymentTransaction = await _paymentService.ProcessPaymentAsync(request.OrderId, request.Amount);
                return Ok(paymentTransaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while processing payment for order {request.OrderId}.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing the payment.");
            }
        }

        [HttpGet("{transactionId}")]
        public async Task<IActionResult> GetPaymentStatus(Guid transactionId)
        {
            try
            {
                var paymentTransaction = await _paymentService.GetPaymentStatusAsync(transactionId);

                if (paymentTransaction == null)
                {
                    return NotFound($"Payment transaction not found for transaction ID {transactionId}.");
                }

                return Ok(paymentTransaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while fetching payment status for transaction {transactionId}.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching the payment status.");
            }
        }
    }
}
