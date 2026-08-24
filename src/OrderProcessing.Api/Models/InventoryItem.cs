using System.ComponentModel.DataAnnotations;

namespace OrderProcessing.Api.Models
{
    public class InventoryItem
    {
        [Key]
        public string ProductId { get; set; } = string.Empty;
        public int AvailableQuantity{  get; set; }
        public int ReservedQuantity { get; set; }
    }
}
