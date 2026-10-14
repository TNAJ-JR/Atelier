using Atelier.Enums;
using Atelier.Modeles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Atelier.Data.Configurations.ConfigurationConverters;

namespace Atelier.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name").IsRequired();
        builder.Property(x => x.Email).HasColumnName("email").IsRequired();
        builder.Property(x => x.Phone).HasColumnName("phone");
        builder.Property(x => x.City).HasColumnName("city");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at")
                                    .HasConversion(IsoUtcConverter)
                                    .IsRequired();

        builder.HasIndex(x => x.Email).IsUnique();
    }
}
