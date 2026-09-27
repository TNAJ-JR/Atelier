using Atelier.Modeles;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Data
{
    public class AtelierDbContext : DbContext
    {
        public AtelierDbContext(DbContextOptions<AtelierDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderTotal> OrderTotals { get; set; }
    }
}
