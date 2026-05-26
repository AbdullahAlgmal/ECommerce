namespace BusinessLayer.DTOs.ProductImage
{
    public class ProductImageDto
    {
        public int Id { get; set; }
        public string Url { get; set; } = null!;
        public byte ImageOrder { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public bool IsPrimary => ImageOrder == 1;
    }
}
