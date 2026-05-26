using BusinessLayer.Interfaces.Repositories;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccessLayer.Repositories
{
    public class ShippingRepository : IShippingRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Shipping> _dbSet;

        public ShippingRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<Shipping>();
        }

        public async Task<Shipping?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(s => s.Order)
                .Include(s => s.Address)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        public async Task<IEnumerable<Shipping>> GetAllAsync()
        {
            return await _dbSet
                .Include(s => s.Order)
                .Include(s => s.Address)
                .OrderByDescending(s => s.ShippingDate)
                .ToListAsync();
        }
        public async Task<Shipping> AddAsync(Shipping entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task<Shipping> UpdateAsync(Shipping entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var shipping = await GetByIdAsync(id);
            if (shipping == null)
                return false;

            _dbSet.Remove(shipping);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<Shipping>> FindAsync(Expression<Func<Shipping, bool>> predicate)
        {
            return await _dbSet
                .Include(s => s.Order)
                .Include(s => s.Address)
                .Where(predicate)
                .ToListAsync();
        }
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
        public async Task<bool> ExistsAsync(Expression<Func<Shipping, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }
    }
}
