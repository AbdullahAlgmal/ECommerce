using System.ComponentModel.DataAnnotations;

namespace CoreLayer.DTOs.ProductImage
{
    public class CreateProductImageDto
    {
        [Required]
        [Url]
        [StringLength(400)]
        public string Url { get; set; } = null!;

        [Required]
        [Range(1, 255)]
        public byte ImageOrder { get; set; }

        [Required]
        public int ProductId { get; set; }
    }
}
