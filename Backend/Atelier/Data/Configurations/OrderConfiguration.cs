using Atelier.Enums;
using Atelier.Modeles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Atelier.Data.Configurations.ConfigurationConverters;

namespace Atelier.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status")
                                 .HasConversion(LowerCaseEnum<OrderStatus>())
                                 .IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at")
                                    .HasConversion(IsoUtcConverter)
                                    .IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at")
                                    .HasConversion(IsoUtcConverter)
                                    .IsRequired();

        builder.HasOne(x => x.Customer)
         .WithMany(c => c.Orders)
         .HasForeignKey(x => x.CustomerId)
         .OnDelete(DeleteBehavior.Restrict);

        // Relation 1-1 vers la vue. En lecture seule : ne jamais assigner
        // Order.Total en écriture, EF tenterait un INSERT sur une vue.
        builder.HasOne(x => x.Total)
         .WithOne()
         .HasForeignKey<OrderTotal>(t => t.OrderId);

        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => new { x.Status, x.CreatedAt });
    }
}
