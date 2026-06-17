using BusinessLayer.DTOs.ProductImage;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DataAccessLayer.Data
{
    [Keyless]
    public class ProductRaw
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public string? ImagesJson { get; set; }
        public string? ReviewsSummaryJson { get; set; }

        public List<ProductImageDto> GetImages()
        {
            if (string.IsNullOrEmpty(ImagesJson))
                return new List<ProductImageDto>();

            try
            {
                return JsonSerializer.Deserialize<List<ProductImageDto>>(ImagesJson)
                       ?? new List<ProductImageDto>();
            }
            catch (JsonException)
            {
                return new List<ProductImageDto>();
            }
        }
        public (double AverageRating, int ReviewCount) GetReviewsSummary()
        {
            if (string.IsNullOrEmpty(ReviewsSummaryJson))
                return (0, 0);

            try
            {
                var summary = JsonSerializer.Deserialize<ReviewsSummary>(ReviewsSummaryJson);
                return (summary?.AverageRating ?? 0, summary?.ReviewCount ?? 0);
            }
            catch (JsonException)
            {
                return (0, 0);
            }
        }
    }

    public class ReviewsSummary
    {
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
}
