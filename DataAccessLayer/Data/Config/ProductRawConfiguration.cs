
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace DataAccessLayer.Data.Config
{
    internal class ProductRawConfiguration : IEntityTypeConfiguration<ProductRaw>
    {
        public void Configure(EntityTypeBuilder<ProductRaw> builder)
        {
            builder.HasNoKey();
            builder.ToView(null); // Not mapped to a database object

            // Map properties to columns
            builder.Property(e => e.Id).HasColumnName("Id");
            builder.Property(e => e.Name).HasColumnName("Name");
            builder.Property(e => e.Description).HasColumnName("Description");
            builder.Property(e => e.Price).HasColumnName("Price");
            builder.Property(e => e.Quantity).HasColumnName("Quantity");
            builder.Property(e => e.CategoryId).HasColumnName("CategoryId");
            builder.Property(e => e.CategoryName).HasColumnName("CategoryName");
            builder.Property(e => e.ImagesJson).HasColumnName("ImagesJson");
            builder.Property(e => e.ReviewsSummaryJson).HasColumnName("ReviewsSummaryJson");
        }
    }
}
