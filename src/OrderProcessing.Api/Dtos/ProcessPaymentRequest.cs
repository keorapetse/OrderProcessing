namespace OrderProcessing.Api.Dtos
{
    public class ProcessPaymentRequest
    {
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
    }
}
