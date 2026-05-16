using CoreLayer.DTOs.Shipping;

namespace CoreLayer.Interfaces.Services
{
    public interface IShippingService
    {
        Task<IEnumerable<ShippingDto>> GetAllShippingsAsync();
        Task<ShippingDto?> GetShippingByIdAsync(int id);
        Task<ShippingDto> CreateShippingAsync(CreateShippingDto createDto);
        Task<ShippingDto> UpdateShippingStatusAsync(int id, UpdateShippingStatusDto updateDto);
        Task<bool> CancelShippingAsync(int id);
        Task<ShippingDto?> GetShippingByOrderAsync(int orderId);
        Task<ShippingTrackingResult> TrackShipmentAsync(string trackingNumber);
        Task<ShippingRateResult> CalculateShippingRateAsync(int addressId, decimal totalWeight, int totalItems);
    }
}
