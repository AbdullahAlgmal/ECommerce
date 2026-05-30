using BusinessLayer.Interfaces.Repositories;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccessLayer.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<User?> GetUserWithAddressesAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _dbSet
                .SingleOrDefaultAsync(u => u.Email == email);
        }
        public async Task<bool> IsEmailUniqueAsync(string email, int? excludeUserId = null)
        {
            var query = _dbSet.Where(u => u.Email == email);

            if (excludeUserId.HasValue)
                query = query.Where(u => u.Id != excludeUserId.Value);

            return !await query.AnyAsync();
        }
    }
}
