using System.ComponentModel.DataAnnotations;

namespace CoreLayer.DTOs.Shipping
{
    public class UpdateShippingStatusDto
    {
        [Required]
        [Range(1, 8)]
        public byte Status { get; set; }

        public DateOnly? DeliveryDate { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
    }
}
