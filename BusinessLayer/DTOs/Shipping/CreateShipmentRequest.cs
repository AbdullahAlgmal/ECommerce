namespace BusinessLayer.DTOs.Shipping
{
    public class CreateShipmentRequest
    {
        public int OrderId { get; set; }
        public int AddressId { get; set; }
        public string CarrierName { get; set; } = null!;
        public DateOnly? ShippingDate { get; set; }
    }
}
