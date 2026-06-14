using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace DataAccessLayer.Data.Config
{
    internal class OrderAggregatedRawConfiguration : IEntityTypeConfiguration<OrderAggregatedRaw>
    {
        public void Configure(EntityTypeBuilder<OrderAggregatedRaw> builder)
        {
            // Configure OrderAggregatedRaw as a keyless entity type
            builder.HasNoKey();  // Keyless entity (for raw SQL results)
            builder.ToView(null); // Not mapped to any table/view

            // Configure properties
            builder.Property(e => e.OrderId).HasColumnName("OrderId");
            builder.Property(e => e.OrderDate).HasColumnName("OrderDate");
            builder.Property(e => e.TotalAmount).HasColumnName("TotalAmount");
            builder.Property(e => e.Status).HasColumnName("Status");
            builder.Property(e => e.StatusName).HasColumnName("StatusName");
            builder.Property(e => e.UserId).HasColumnName("UserId");
            builder.Property(e => e.UserFullName).HasColumnName("UserFullName");
            builder.Property(e => e.UserEmail).HasColumnName("UserEmail");
            builder.Property(e => e.TotalItems).HasColumnName("TotalItems");
            builder.Property(e => e.OrderItemsJson).HasColumnName("OrderItemsJson");
        }
    }
}
