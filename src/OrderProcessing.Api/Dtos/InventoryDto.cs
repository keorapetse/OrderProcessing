using System.ComponentModel.DataAnnotations;

namespace OrderProcessing.Api.Dtos
{
    public class InventoryDto
    {
        public record StockChangeRequest
        {
            [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
            public int Quantity { get; set; }
        }

        public record InventoryResponse(string ProductId, int AvailableQuantity, int ReservedQuantity, string message);
    }
}
