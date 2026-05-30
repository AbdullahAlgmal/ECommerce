using BusinessLayer.Interfaces.Repositories;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccessLayer.Repositories
{
    public class ShippingRepository : BaseRepository<Shipping>, IShippingRepository
    {
        public ShippingRepository(AppDbContext context) : base(context) { }

        public async Task<Shipping?> GetShippingByOrderAsync(int orderId)
        {
            return await _dbSet
                .Include(s => s.Address)
                .FirstOrDefaultAsync(s => s.OrderId == orderId);
        }
        public async Task<Shipping?> GetShippingByTrackingNumberAsync(string trackingNumber)
        {
            return await _dbSet
                .Include(s => s.Order)
                .Include(s => s.Address)
                .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber);
        }
        public async Task<IEnumerable<Shipping>> GetShippingsByAddressAsync(int addressId)
        {
            return await _dbSet
                .Include(s => s.Order)
                .Where(s => s.AddressId == addressId)
                .OrderByDescending(s => s.ShippingDate)
                .ToListAsync();
        }
    }
}
