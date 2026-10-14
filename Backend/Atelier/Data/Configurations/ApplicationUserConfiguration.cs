using Atelier.Enums;
using Atelier.Modeles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Atelier.Data.Configurations.ConfigurationConverters;

namespace Atelier.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Email).HasColumnName("email").IsRequired();
        builder.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
        builder.Property(x => x.Role).HasColumnName("role")
                               .HasConversion(LowerCaseEnum<UserRole>())
                               .IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at")
                                    .HasConversion(IsoUtcConverter)
                                    .IsRequired();

        builder.HasIndex(x => x.Email).IsUnique();
    }
}
