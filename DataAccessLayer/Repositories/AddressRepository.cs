using BusinessLayer.Interfaces.Repositories;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccessLayer.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Address> _dbSet;

        public AddressRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<Address>();
        }

        public async Task<Address?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }
        public async Task<Address> AddAsync(Address entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task<Address> UpdateAsync(Address entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var address = await GetByIdAsync(id);
            if (address == null)
                return false;

            _dbSet.Remove(address);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Address>> FindAsync(Expression<Func<Address, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }
        public async Task<IEnumerable<Address>> GetAddressesByUserAsync(int userId)
        {
            return await _dbSet
                .Where(a => a.UserId == userId)
                .ToListAsync();
        }
        public async Task<Address?> GetAddressWithUserAsync(int addressId)
        {
            return await _dbSet
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == addressId);
        }
        public async Task<bool> UserOwnsAddressAsync(int addressId, int userId)
        {
            return await _dbSet
                .AnyAsync(a => a.Id == addressId && a.UserId == userId);
        }
        public async Task<bool> ExistsAsync(Expression<Func<Address, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task<bool> DeleteAddressesByUserAsync(int userId)
        {
            var addresses = await _dbSet.Where(a => a.UserId == userId).ToListAsync();
            if (!addresses.Any())
                return false;

            _dbSet.RemoveRange(addresses);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
