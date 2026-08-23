using Microsoft.EntityFrameworkCore;
using OrderProcessing.Api.Models;


namespace OrderProcessing.Api.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
        public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //one order can have many order items, but each order item belongs to one order
            modelBuilder.Entity<Order>().HasMany(o => o.Items).WithOne().HasForeignKey(oi => oi.OrderId);
            modelBuilder.Entity<InventoryItem>().HasKey(i => i.ProductId);
            modelBuilder.Entity<PaymentTransaction>().HasKey(t => t.TransactionId);
        }
    }
}
