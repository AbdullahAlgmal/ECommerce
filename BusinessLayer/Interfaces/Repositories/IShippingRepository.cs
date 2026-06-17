using BusinessLayer.DTOs.Shipping;
using DataAccessLayer;
using System.Linq.Expressions;

namespace BusinessLayer.Interfaces.Repositories
{
    public interface IShippingRepository : IBaseRepository<Shipping>
    {
        new Task<IEnumerable<ShippingDto>> GetAllAsync();
        new Task<ShippingDto?> GetByIdAsync(int id);
        Task<ShippingDto?> GetShippingByOrderAsync(int orderId);
        Task<ShippingDto?> GetShippingByTrackingNumberAsync(string trackingNumber);
        Task<IEnumerable<ShippingDto>> GetShippingsByAddressAsync(int addressId);
    }
}
