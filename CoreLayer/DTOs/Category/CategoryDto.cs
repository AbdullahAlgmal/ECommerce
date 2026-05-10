namespace CoreLayer.DTOs.Category
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int ProductCount { get; set; }
        public List<ProductBriefDto> Products { get; set; } = new();
    }
}
