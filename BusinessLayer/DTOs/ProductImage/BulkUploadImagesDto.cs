using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.DTOs.ProductImage
{
    public class BulkUploadImagesDto
    {
        [Required]
        public List<string> ImageUrls { get; set; } = new();

        [Required]
        public int ProductId { get; set; }
    }
}
