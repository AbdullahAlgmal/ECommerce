namespace BusinessLayer.DTOs.Shipping
{
    public class ShippingRateRequest
    {
        public int AddressId { get; set; }
        public decimal TotalWeight { get; set; }
        public int TotalItems { get; set; }
    }
}
