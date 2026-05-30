using DataAccessLayer;
using System.Linq.Expressions;

namespace BusinessLayer.Interfaces.Repositories
{
    public interface IShippingRepository : IBaseRepository<Shipping>
    {
        Task<Shipping?> GetShippingByOrderAsync(int orderId);
        Task<Shipping?> GetShippingByTrackingNumberAsync(string trackingNumber);
        Task<IEnumerable<Shipping>> GetShippingsByAddressAsync(int addressId);
    }
}
