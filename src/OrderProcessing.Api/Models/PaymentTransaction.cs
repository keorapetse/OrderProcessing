namespace OrderProcessing.Api.Models
{
    public class PaymentTransaction
    {
        public Guid TransactionId { get; set; } = Guid.NewGuid();
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
        public string? FailureReason { get; set; }
    }
}
