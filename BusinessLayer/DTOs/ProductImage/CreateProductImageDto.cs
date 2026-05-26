using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.DTOs.ProductImage
{
    public class CreateProductImageDto
    {
        [Required]
        [Url]
        [StringLength(400, MinimumLength = 1)]
        public string Url { get; set; } = null!;

        [Range(1, 255)]
        public byte? ImageOrder { get; set; }

        [Required]
        public int ProductId { get; set; }
    }
}
