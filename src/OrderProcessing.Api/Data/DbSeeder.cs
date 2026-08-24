using OrderProcessing.Api.Models;

namespace OrderProcessing.Api.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            // Seed InventoryItems only if the database is empty
            if (context.InventoryItems.Any())
            {
                return;
            }

            context.InventoryItems.AddRange
            (
                new InventoryItem
                {
                    ProductId = "LAPTOP",
                    AvailableQuantity = 25,
                    ReservedQuantity = 0
                },
                new InventoryItem
                {
                    ProductId = "MOUSE",
                    AvailableQuantity = 100,
                    ReservedQuantity = 0
                },
                new InventoryItem
                {
                    ProductId = "KEYBOARD",
                    AvailableQuantity = 40,
                    ReservedQuantity = 0
                },
                new InventoryItem
                {
                    ProductId = "MONITOR",
                    AvailableQuantity = 10,
                    ReservedQuantity = 0
                },
                new InventoryItem
                {
                    ProductId = "HEADSET",
                    AvailableQuantity = 3,
                    ReservedQuantity = 0
                },
                new InventoryItem
                {
                    ProductId = "WEBCAM",
                    AvailableQuantity = 1,
                    ReservedQuantity = 0
                },
                new InventoryItem
                {
                    ProductId = "DOCK",
                    AvailableQuantity = 0,
                    ReservedQuantity = 0
                },
                new InventoryItem
                {
                    ProductId = "CABLE",
                    AvailableQuantity = 500,
                    ReservedQuantity = 0
                }
            );

            context.SaveChanges();
        }
    }
}