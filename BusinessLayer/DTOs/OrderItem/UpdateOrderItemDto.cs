using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.DTOs.OrderItem
{
    public class UpdateOrderItemDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, 999999.99)]
        public decimal Price { get; set; }
    }
}
