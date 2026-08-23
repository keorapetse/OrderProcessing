namespace OrderProcessing.Api.Dtos
{
    public class CreateOrderRequest
    {
        public string CustomerId { get; set; } = string.Empty;
        public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();>
    }
}
