namespace OrderProcessing.Api.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public Guid OrderId { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; } 
        public decimal UnitPrice { get; set; }
    }
}
