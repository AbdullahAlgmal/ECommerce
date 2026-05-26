namespace BusinessLayer.DTOs.Shipping
{
    public class ShippingTrackingResult
    {
        public bool IsSuccess { get; set; }
        public string TrackingNumber { get; set; } = null!;
        public string Status { get; set; } = null!;
        public List<TrackingEvent> TrackingHistory { get; set; } = new();
    }
}
