namespace BusinessLayer.DTOs.Shipping
{
    public class ShippingRateResult
    {
        public string CarrierName { get; set; } = null!;
        public decimal Rate { get; set; }
        public int EstimatedDays { get; set; }
    }
}
