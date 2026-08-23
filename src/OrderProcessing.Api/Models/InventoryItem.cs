using Microsoft.AspNetCore.Http.Connections;

namespace OrderProcessing.Api.Models
{
    public class InventoryItem
    {
        public string ProductId { get; set; } = default!;
        public int AvailableQuantity{  get; set; }
        public int ReservedQuantity { get; set; }
    }
}
