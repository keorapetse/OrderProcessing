using Microsoft.EntityFrameworkCore;
using OrderProcessing.Api.Data;
using OrderProcessing.Api.Interfaces;
using OrderProcessing.Api.Models;

namespace OrderProcessing.Api.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(AppDbContext appDbContext, ILogger<PaymentService> logger)
        {
            _db = appDbContext;
            _logger = logger;
        }

        public async Task<PaymentTransaction> ProcessPaymentAsync(Guid orderId, decimal amount)
        {
            var paymentTransaction = new PaymentTransaction
            {
                TransactionId = Guid.NewGuid(),
                OrderId = orderId,
                Amount = amount,
                Status = PaymentStatus.Completed,
                ProcessedAt = DateTime.UtcNow
            };

            _db.PaymentTransactions.Add(paymentTransaction);
            await _db.SaveChangesAsync();
            return paymentTransaction;
        }

        public async Task<PaymentTransaction> GetPaymentStatusAsync(Guid transactionId)
        {
            var paymentTransaction = await _db.PaymentTransactions.FirstOrDefaultAsync(x => x.TransactionId == transactionId);
            return paymentTransaction;
        }
    }
}
