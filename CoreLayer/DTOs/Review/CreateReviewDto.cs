using System.ComponentModel.DataAnnotations;

namespace CoreLayer.DTOs.Review
{
    public class CreateReviewDto
    {
        [Required]
        [StringLength(500, MinimumLength = 3)]
        public string ReviewText { get; set; } = null!;

        [Required]
        [Range(0.5, 5.0)]
        public decimal Rating { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int UserId { get; set; }
    }
}
