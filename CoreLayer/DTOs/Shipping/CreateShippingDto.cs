using System.ComponentModel.DataAnnotations;

namespace CoreLayer.DTOs.Shipping
{
    public class CreateShippingDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public int AddressId { get; set; }

        [Required]
        [StringLength(100)]
        public string CarrierName { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string TrackingNumber { get; set; } = null!;

        public DateOnly? ShippingDate { get; set; }
        public DateOnly? DeliveryDate { get; set; }
    }
}
