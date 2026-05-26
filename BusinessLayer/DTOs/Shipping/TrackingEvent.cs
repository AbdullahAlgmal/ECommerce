namespace BusinessLayer.DTOs.Shipping
{
    public class TrackingEvent
    {
        public DateTime Date { get; set; }
        public string Status { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
