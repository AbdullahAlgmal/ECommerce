using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.DTOs.Order
{
    public class UpdateOrderStatusDto
    {
        [Required]
        [Range(1, 6, ErrorMessage = "Status must be between 1 and 6.")]
        public byte Status { get; set; }
    }
}
