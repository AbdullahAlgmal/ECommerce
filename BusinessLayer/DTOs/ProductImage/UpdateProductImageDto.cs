using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.DTOs.ProductImage
{
    public class UpdateProductImageDto
    {
        [Required]
        [Url]
        [StringLength(400, MinimumLength = 1)]
        public string Url { get; set; } = null!;

        [Required]
        [Range(1, 255)]
        public byte ImageOrder { get; set; }
    }
}
