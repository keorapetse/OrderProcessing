using OrderProcessing.Api.Models;

namespace OrderProcessing.Api.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentTransaction> ProcessPaymentAsync(Guid orderId, decimal amount);
        Task<PaymentTransaction> GetPaymentStatusAsync(Guid transactionId);

    }
}
