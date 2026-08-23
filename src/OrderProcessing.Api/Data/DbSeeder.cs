using OrderProcessing.Api.Models;
namespace OrderProcessing.Api.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            // Seed InventoryItems
            if (!context.InventoryItems.Any()) return;

            context.InventoryItems.AddRange
            (

                new InventoryItem
                {
                    ProductId = "LAPTOP-001",
                    AvailableQuantity = 25,
                    ReservedQuantity = 0
                },
                new InventoryItem
                {
                    ProductId = "MOUSE-002",
                    AvailableQuantity = 100,
                    ReservedQuantity = 0
                },
                new InventoryItem
                {
                    ProductId = "KEYBOARD-003",
                    AvailableQuantity = 40,
                    ReservedQuantity = 0
                },
                 new InventoryItem
                 {
                     ProductId = "MONITOR-004",
                     AvailableQuantity = 10,
                     ReservedQuantity = 0
                 },
                new InventoryItem
                {
                    ProductId = "HEADSET-005",
                    AvailableQuantity = 3,
                    ReservedQuantity = 0
                },
                new InventoryItem
                {
                    ProductId = "WEBCAM-006",
                    AvailableQuantity = 1,
                    ReservedQuantity = 0
                },
                 new InventoryItem
                 {
                     ProductId = "DOCK-007",
                     AvailableQuantity = 0,
                     ReservedQuantity = 0
                 },
                new InventoryItem
                {
                    ProductId = "CABLE-008",
                    AvailableQuantity = 500,
                    ReservedQuantity = 0
                });
            context.SaveChanges();
        }
    }
}


