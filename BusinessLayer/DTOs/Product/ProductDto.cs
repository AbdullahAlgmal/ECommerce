using BusinessLayer.DTOs.ProductImage;

namespace BusinessLayer.DTOs.Product
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public List<ProductImageDto> Images { get; set; } = new();
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
}
