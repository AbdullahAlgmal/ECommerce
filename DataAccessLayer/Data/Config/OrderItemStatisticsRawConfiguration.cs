using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Data.Config
{
    public class OrderItemStatisticsRawConfiguration : IEntityTypeConfiguration<OrderItemStatisticsRaw>
    {
        public void Configure(EntityTypeBuilder<OrderItemStatisticsRaw> builder)
        {
            // Mark as keyless entity (no primary key)
            builder.HasNoKey();

            // Specify that this entity is not mapped to any table/view
            builder.ToView(null);

            // Configure property mappings
            builder.Property(e => e.TotalItemsSold)
                .HasColumnName("TotalItemsSold")
                .HasDefaultValue(0);

            builder.Property(e => e.TotalRevenue)
                .HasColumnName("TotalRevenue")
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            builder.Property(e => e.AverageOrderValue)
                .HasColumnName("AverageOrderValue")
                .HasPrecision(18, 2)
                .HasDefaultValue(0);

            builder.Property(e => e.UniqueProductsSold)
                .HasColumnName("UniqueProductsSold")
                .HasDefaultValue(0);

            builder.Property(e => e.TopCategoriesJson)
                .HasColumnName("TopCategoriesJson")
                .HasColumnType("nvarchar(max)");
        }

    }
}