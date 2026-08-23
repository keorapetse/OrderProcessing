namespace OrderProcessing.Api.Models
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string CustomerId { get; set; } = default!; //what is default?
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow; //what is datetimeoffset and not use the regualr datetime    ?
        public DateTimeOffset UpdatedAt { get; set;} = DateTimeOffset.UtcNow;
    }
}
