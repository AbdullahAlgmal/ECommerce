using BusinessLayer.Interfaces.Repositories;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccessLayer.Repositories
{
    public class AddressRepository : BaseRepository<Address>, IAddressRepository
    {
        public AddressRepository(AppDbContext context) : base(context) { }

        public async Task<Address?> GetAddressByUserAsync(int userId)
        {
            return await _dbSet
                .Where(a => a.UserId == userId)
                .FirstOrDefaultAsync();
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
