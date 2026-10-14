using Atelier.Enums;
using Atelier.Modeles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using static Atelier.Data.Configurations.ConfigurationConverters;

namespace Atelier.Data.Configurations;

public class OrderTotalConfiguration : IEntityTypeConfiguration<OrderTotal>
{
    public void Configure(EntityTypeBuilder<OrderTotal> builder)
    {
        builder.ToView("order_totals");
        builder.HasKey(x => x.OrderId);
        builder.Property(x => x.OrderId).HasColumnName("order_id");
        builder.Property(x => x.TotalCents).HasColumnName("total_cents");
        builder.Property(x => x.LineCount).HasColumnName("line_count");
    }
}
