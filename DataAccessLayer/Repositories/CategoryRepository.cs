using CoreLayer.Interfaces.Repositories;
using ECommerceApi;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccessLayer.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Category> _dbSet;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<Category>();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.Products)
                .ToListAsync();
        }
        public async Task<Category> AddAsync(Category entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task<Category> UpdateAsync(Category entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var category = await GetByIdAsync(id);
            if (category == null)
                return false;

            _dbSet.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Category>> FindAsync(Expression<Func<Category, bool>> predicate)
        {
            return await _dbSet
                .Include(c => c.Products)
                .Where(predicate)
                .ToListAsync();
        }
        public async Task<Category?> GetCategoryWithProductsAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
        public async Task<bool> ExistsAsync(Expression<Func<Category, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }
        public async Task<bool> IsCategoryNameUniqueAsync(string name, int? excludeId = null)
        {
            var query = _dbSet.Where(c => c.Name.ToLower() == name.ToLower());

            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);

            return !await query.AnyAsync();
        }

        public async Task<int> CountAsync(Expression<Func<Category, bool>>? predicate = null)
        {
            if (predicate == null)
                return await _dbSet.CountAsync();

            return await _dbSet.CountAsync(predicate);
        }
        public async Task<Dictionary<int, int>> GetProductCountPerCategoryAsync()
        {
            return await _dbSet
                .Include(c => c.Products)
                .ToDictionaryAsync(c => c.Id, c => c.Products.Count);
        }
    }
}
