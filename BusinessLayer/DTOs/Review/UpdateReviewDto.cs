using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.DTOs.Review
{
    public class UpdateReviewDto
    {
        [Required]
        [StringLength(500, MinimumLength = 3)]
        public string ReviewText { get; set; } = null!;

        [Required]
        [Range(0.5, 5.0)]
        public decimal Rating { get; set; }
    }
}
