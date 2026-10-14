using Atelier.Enums;
using Atelier.Modeles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using static Atelier.Data.Configurations.ConfigurationConverters;

namespace Atelier.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("products");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Sku).HasColumnName("sku").IsRequired();
            builder.Property(x => x.Name).HasColumnName("name").IsRequired();
            builder.Property(x => x.PriceCents).HasColumnName("price_cents").IsRequired();
            builder.Property(x => x.Stock).HasColumnName("stock").IsRequired();
            builder.Property(x => x.Category).HasColumnName("category")
                .HasConversion(LowerCaseEnum<ProductCategory>())
                .IsRequired();
            builder.Property(x => x.Active).HasColumnName("active").IsRequired();
            builder.Property(x => x.CreatedAt).HasColumnName("created_at")
                .HasConversion(IsoUtcConverter)
                .IsRequired();

            builder.HasIndex(x => x.Sku).IsUnique();
            builder.HasIndex(x => x.Category);
            builder.HasIndex(x => x.PriceCents);
        }
    }
}
