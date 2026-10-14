using Atelier.Enums;
using Atelier.Modeles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Atelier.Data.Configurations.ConfigurationConverters;

namespace Atelier.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");
        builder.HasKey(x => new { x.OrderId, x.ProductId });
        builder.Property(x => x.OrderId).HasColumnName("order_id");
        builder.Property(x => x.ProductId).HasColumnName("product_id");
        builder.Property(x => x.Quantity).HasColumnName("quantity").IsRequired();
        builder.Property(x => x.UnitPriceCents).HasColumnName("unit_price_cents").IsRequired();

        builder.Ignore(x => x.LineTotalCents); // calculé, pas de colonne

        builder.HasOne(x => x.Order)
         .WithMany(o => o.Items)
         .HasForeignKey(x => x.OrderId)
         .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product)
         .WithMany(p => p.OrderItems)
         .HasForeignKey(x => x.ProductId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
