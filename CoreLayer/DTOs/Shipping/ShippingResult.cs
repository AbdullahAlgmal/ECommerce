namespace CoreLayer.DTOs.Shipping
{
    public class ShippingResult
    {
        public bool IsSuccess { get; set; }
        public string TrackingNumber { get; set; } = null!;
        public string CarrierName { get; set; } = null!;
        public DateOnly EstimatedDeliveryDate { get; set; }
        public string Status { get; set; } = null!;
        public string Message { get; set; } = null!;
    }
}
