namespace BusinessLayer.DTOs.Shipping
{
    public class ShippingDto
    {
        public int Id { get; set; }
        public DateOnly ShippingDate { get; set; }
        public DateOnly DeliveryDate { get; set; }
        public byte Status { get; set; }
        public string StatusName { get; set; } = null!;
        public string CarrierName { get; set; } = null!;
        public string TrackingNumber { get; set; } = null!;
        public int AddressId { get; set; }
        public string FullAddress { get; set; } = null!;
        public int OrderId { get; set; }
        public List<ShippingTrackingEventDto> TrackingHistory { get; set; } = new();
    }
}
