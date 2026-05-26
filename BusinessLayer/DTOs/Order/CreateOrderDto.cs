using BusinessLayer.DTOs.OrderItem;
using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.DTOs.Order
{
    public class CreateOrderDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [MinLength(1)]
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }
}
