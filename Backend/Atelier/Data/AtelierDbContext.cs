using Atelier.Enums;
using Atelier.Modeles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;

namespace Atelier.Data;


// DbContext — configuration Fluent API alignée sur schema.sql
//
// Deux points où EF Core diverge de la base par défaut, et que ce fichier
// corrige explicitement :
//
//   1. Les enums. HasConversion<string>() écrirait "Draft", "Admin", "Bois"
//      en PascalCase, ce qui violerait les contraintes
//      CHECK (status IN ('draft', ...)). D'où les convertisseurs en minuscules.
//
//   2. Les dates. Le provider SQLite sérialise DateTimeOffset au format
//      "yyyy-MM-dd HH:mm:ss.fffffffzzz", incompatible avec l'ISO 8601 produit
//      par strftime('%Y-%m-%dT%H:%M:%fZ','now') dans schema.sql. Sans le
//      convertisseur ci-dessous, les lignes écrites par EF et celles écrites
//      par le seed SQL ne se trieraient pas ensemble.

public class AtelierDbContext(DbContextOptions<AtelierDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderTotal> OrderTotals => Set<OrderTotal>();

    // 
    // Convertisseurs

    private const string IsoFormat = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";

    private static readonly ValueConverter<DateTimeOffset, string> IsoUtcConverter = new(
        v => v.ToUniversalTime().ToString(IsoFormat, CultureInfo.InvariantCulture),
        v => DateTimeOffset.Parse(
                 v,
                 CultureInfo.InvariantCulture,
                 DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal));

    private static ValueConverter<TEnum, string> LowerCaseEnum<TEnum>() where TEnum : struct, Enum
        => new(
            v => v.ToString()!.ToLowerInvariant(),
            v => Enum.Parse<TEnum>(v, ignoreCase: true));

    protected override void OnModelCreating(ModelBuilder b)
    {
        // users
        b.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Email).HasColumnName("email").IsRequired();
            e.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
            e.Property(x => x.Role).HasColumnName("role")
                                   .HasConversion(LowerCaseEnum<UserRole>())
                                   .IsRequired();
            e.Property(x => x.CreatedAt).HasColumnName("created_at")
                                        .HasConversion(IsoUtcConverter)
                                        .IsRequired();

            e.HasIndex(x => x.Email).IsUnique();
        });

        // products
        b.Entity<Product>(e =>
        {
            e.ToTable("products");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Sku).HasColumnName("sku").IsRequired();
            e.Property(x => x.Name).HasColumnName("name").IsRequired();
            e.Property(x => x.PriceCents).HasColumnName("price_cents").IsRequired();
            e.Property(x => x.Stock).HasColumnName("stock").IsRequired();
            e.Property(x => x.Category).HasColumnName("category")
                                       .HasConversion(LowerCaseEnum<ProductCategory>())
                                       .IsRequired();
            e.Property(x => x.Active).HasColumnName("active").IsRequired();
            e.Property(x => x.CreatedAt).HasColumnName("created_at")
                                        .HasConversion(IsoUtcConverter)
                                        .IsRequired();

            e.HasIndex(x => x.Sku).IsUnique();
            e.HasIndex(x => x.Category);
            e.HasIndex(x => x.PriceCents);
        });

        // customers
        b.Entity<Customer>(e =>
        {
            e.ToTable("customers");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Name).HasColumnName("name").IsRequired();
            e.Property(x => x.Email).HasColumnName("email").IsRequired();
            e.Property(x => x.Phone).HasColumnName("phone");
            e.Property(x => x.City).HasColumnName("city");
            e.Property(x => x.CreatedAt).HasColumnName("created_at")
                                        .HasConversion(IsoUtcConverter)
                                        .IsRequired();

            e.HasIndex(x => x.Email).IsUnique();
        });

        // orders
        b.Entity<Order>(e =>
        {
            e.ToTable("orders");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.CustomerId).HasColumnName("customer_id").IsRequired();
            e.Property(x => x.Status).HasColumnName("status")
                                     .HasConversion(LowerCaseEnum<OrderStatus>())
                                     .IsRequired();
            e.Property(x => x.CreatedAt).HasColumnName("created_at")
                                        .HasConversion(IsoUtcConverter)
                                        .IsRequired();
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at")
                                        .HasConversion(IsoUtcConverter)
                                        .IsRequired();

            e.HasOne(x => x.Customer)
             .WithMany(c => c.Orders)
             .HasForeignKey(x => x.CustomerId)
             .OnDelete(DeleteBehavior.Restrict);

            // Relation 1-1 vers la vue. En lecture seule : ne jamais assigner
            // Order.Total en écriture, EF tenterait un INSERT sur une vue.
            e.HasOne(x => x.Total)
             .WithOne()
             .HasForeignKey<OrderTotal>(t => t.OrderId);

            e.HasIndex(x => x.CustomerId);
            e.HasIndex(x => new { x.Status, x.CreatedAt });
        });

        // order_items — clé primaire composite
        b.Entity<OrderItem>(e =>
        {
            e.ToTable("order_items");
            e.HasKey(x => new { x.OrderId, x.ProductId });
            e.Property(x => x.OrderId).HasColumnName("order_id");
            e.Property(x => x.ProductId).HasColumnName("product_id");
            e.Property(x => x.Quantity).HasColumnName("quantity").IsRequired();
            e.Property(x => x.UnitPriceCents).HasColumnName("unit_price_cents").IsRequired();

            e.Ignore(x => x.LineTotalCents); // calculé, pas de colonne

            e.HasOne(x => x.Order)
             .WithMany(o => o.Items)
             .HasForeignKey(x => x.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Product)
             .WithMany(p => p.OrderItems)
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
        });


        // order_totals — vue, lecture seule
        b.Entity<OrderTotal>(e =>
        {
            e.ToView("order_totals");
            e.HasKey(x => x.OrderId);
            e.Property(x => x.OrderId).HasColumnName("order_id");
            e.Property(x => x.TotalCents).HasColumnName("total_cents");
            e.Property(x => x.LineCount).HasColumnName("line_count");
        });
    }
}