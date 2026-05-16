using ECommerceApi;
using System.Linq.Expressions;

namespace CoreLayer.Interfaces.Repositories
{
    public interface IShippingRepository
    {
        Task<Shipping?> GetByIdAsync(int id);
        Task<IEnumerable<Shipping>> GetAllAsync();
        Task<Shipping> AddAsync(Shipping entity);
        Task<Shipping> UpdateAsync(Shipping entity);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Shipping>> FindAsync(Expression<Func<Shipping, bool>> predicate);
        Task<Shipping?> GetShippingByOrderAsync(int orderId);
        Task<Shipping?> GetShippingByTrackingNumberAsync(string trackingNumber);
        Task<IEnumerable<Shipping>> GetShippingsByAddressAsync(int addressId);
        Task<bool> ExistsAsync(Expression<Func<Shipping, bool>> predicate);
    }
}
