using CoreLayer.DTOs.Shipping;

namespace CoreLayer.Interfaces.Services
{
    public interface ISimulatedShippingService
    {
        Task<ShippingResult> CreateShipmentAsync(CreateShipmentRequest request);
        Task<ShippingTrackingResult> TrackShipmentAsync(string trackingNumber);
        Task<bool> CancelShipmentAsync(string trackingNumber);
        Task<ShippingRateResult> CalculateShippingRateAsync(ShippingRateRequest request);
    }
}
