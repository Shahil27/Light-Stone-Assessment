using Microsoft.EntityFrameworkCore;
using Light_Stone_Assessment.Models;

namespace Light_Stone_Assessment.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(b =>
            {
                b.HasKey(p => p.Sku);
                b.Property(p => p.Price).HasPrecision(18, 2);
                b.Property(p => p.Stock).IsRequired();
            });

            modelBuilder.Entity<Order>(b =>
            {
                b.HasKey(o => o.Id);
                b.HasIndex(o => o.ExternalOrderId).IsUnique();
                b.Property(o => o.PlacedAt).IsRequired();
            });

            modelBuilder.Entity<OrderItem>(b =>
            {
                b.HasKey(oi => oi.Id);
                b.Property(oi => oi.UnitPrice).HasPrecision(18, 2);
                b.HasOne(oi => oi.Order).WithMany(o => o.Items).HasForeignKey(oi => oi.OrderId);
                b.HasOne(oi => oi.Product).WithMany().HasForeignKey(oi => oi.Sku);
            });
        }
    }
}
