using CoreLayer.DTOs.OrderItem;

namespace CoreLayer.DTOs.Order
{
    public class OrderDto
    {
        public int Id { get; set; }
        public DateOnly OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public byte Status { get; set; }
        public string StatusName { get; set; } = null!;
        public int UserId { get; set; }
        public string UserFullName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public List<OrderItemDto> OrderItems { get; set; } = new();
        public int TotalItems { get; set; }
    }
}
