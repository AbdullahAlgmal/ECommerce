using System.ComponentModel.DataAnnotations;

namespace CoreLayer.DTOs.OrderItem
{
    public class CreateOrderItemDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
