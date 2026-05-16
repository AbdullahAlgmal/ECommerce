using CoreLayer.Interfaces.Repositories;
using ECommerceApi;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccessLayer.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<User> _dbSet;

        public UserRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<User>();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }
        public async Task<User> AddAsync(User entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task<User> UpdateAsync(User entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var user = await GetByIdAsync(id);
            if (user == null)
                return false;

            _dbSet.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
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
        public async Task<bool> ExistsAsync(Expression<Func<User, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }
        public async Task<int> CountAsync(Expression<Func<User, bool>>? predicate = null)
        {
            if (predicate == null)
                return await _dbSet.CountAsync();

            return await _dbSet.CountAsync(predicate);
        }

        public async Task<IEnumerable<User>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<User, bool>>? predicate = null)
        {
            var query = _dbSet.AsQueryable();

            if (predicate != null)
                query = query.Where(predicate);

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
