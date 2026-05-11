using System.ComponentModel.DataAnnotations;

namespace CoreLayer.DTOs.ProductImage
{
    public class ReorderImagesDto
    {
        [Required]
        public Dictionary<int, byte> ImageOrders { get; set; } = new();
    }
}
