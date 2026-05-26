using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.DTOs.Product
{
    public class UpdateProductDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = null!;

        [StringLength(500)]
        public string Description { get; set; } = null!;

        [Required]
        [Range(0.01, 999999.99)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }
}
