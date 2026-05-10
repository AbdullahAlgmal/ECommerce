using System.ComponentModel.DataAnnotations;

namespace CoreLayer.DTOs.Category
{
    public class UpdateCategoryDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = null!;
    }
}
