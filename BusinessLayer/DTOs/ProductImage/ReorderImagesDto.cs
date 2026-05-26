using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.DTOs.ProductImage
{
    public class ReorderImagesDto
    {
        [Required]
        public Dictionary<int, byte> ImageOrders { get; set; } = new();
    }
}
