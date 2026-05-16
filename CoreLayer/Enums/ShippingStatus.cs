namespace CoreLayer.Enums
{
    public enum ShippingStatus : byte
    {
        Pending = 1,
        Processing = 2,
        Shipped = 3,
        InTransit = 4,
        OutForDelivery = 5,
        Delivered = 6,
        Cancelled = 7,
        Failed = 8
    }

    public static class ShippingStatusExtensions
    {
        public static string GetStatusName(this ShippingStatus status)
        {
            return status switch
            {
                ShippingStatus.Pending => "Pending",
                ShippingStatus.Processing => "Processing",
                ShippingStatus.Shipped => "Shipped",
                ShippingStatus.InTransit => "In Transit",
                ShippingStatus.OutForDelivery => "Out for Delivery",
                ShippingStatus.Delivered => "Delivered",
                ShippingStatus.Cancelled => "Cancelled",
                ShippingStatus.Failed => "Failed",
                _ => "Unknown"
            };
        }
    }
}
