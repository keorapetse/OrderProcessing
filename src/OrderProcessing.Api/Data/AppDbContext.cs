using Microsoft.EntityFrameworkCore;
using OrderProcessing.Api.Models;


namespace OrderProcessing.Api.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
        public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();

    }
}
