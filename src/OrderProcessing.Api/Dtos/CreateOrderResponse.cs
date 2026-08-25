namespace OrderProcessing.Api.Dtos
{
    public class CreateOrderResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public OrderResponseDto? Order { get; set; }
    }
}