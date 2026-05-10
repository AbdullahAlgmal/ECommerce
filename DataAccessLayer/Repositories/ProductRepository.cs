using CoreLayer.Interfaces.Repositories;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccessLayer.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Product> _dbSet;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<Product>();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }
        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.Reviews)
                .ToListAsync();
        }
        public async Task<Product> AddAsync(Product entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task<Product> UpdateAsync(Product entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var product = await GetByIdAsync(id);
            if (product == null)
                return false;

            _dbSet.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Product>> FindAsync(Expression<Func<Product, bool>> predicate)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(predicate)
                .ToListAsync();
        }
        public async Task<Product?> GetProductWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
        }
        public async Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
                .OrderBy(p => p.Price)
                .ToListAsync();
        }
        public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.Quantity <= threshold)
                .OrderBy(p => p.Quantity)
                .ToListAsync();
        }
        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            var term = searchTerm.ToLower();
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.Name.ToLower().Contains(term) ||
                           p.Description.ToLower().Contains(term))
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<Product, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }
        public async Task<bool> IsProductNameUniqueAsync(string name, int? excludeId = null)
        {
            var query = _dbSet.Where(p => p.Name.ToLower() == name.ToLower());

            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);

            return !await query.AnyAsync();
        }

        public async Task<int> CountAsync(Expression<Func<Product, bool>>? predicate = null)
        {
            if (predicate == null)
                return await _dbSet.CountAsync();

            return await _dbSet.CountAsync(predicate);
        }
        public async Task<decimal> GetAverageProductPriceAsync()
        {
            return await _dbSet.AverageAsync(p => p.Price);
        }
        public async Task<(IEnumerable<Product> Products, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<Product, bool>>? predicate = null,
            string? sortBy = null,
            bool sortDescending = false)
        {
            var query = _dbSet
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .AsQueryable();

            if (predicate != null)
                query = query.Where(predicate);

            var totalCount = await query.CountAsync();

            query = ApplySorting(query, sortBy, sortDescending);

            var products = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalCount);
        }

        public async Task<bool> UpdateStockAsync(int productId, int quantity)
        {
            var product = await GetByIdAsync(productId);
            if (product == null)
                return false;

            product.Quantity = quantity;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> ReduceStockAsync(int productId, int quantity)
        {
            var product = await GetByIdAsync(productId);
            if (product == null || product.Quantity < quantity)
                return false;

            product.Quantity -= quantity;
            await _context.SaveChangesAsync();
            return true;
        }

        private IQueryable<Product> ApplySorting(IQueryable<Product> query, string? sortBy, bool sortDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(p => p.Id);

            return sortBy.ToLower() switch
            {
                "name" => sortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "price" => sortDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                "quantity" => sortDescending ? query.OrderByDescending(p => p.Quantity) : query.OrderBy(p => p.Quantity),
                "category" => sortDescending ? query.OrderByDescending(p => p.Category.Name) : query.OrderBy(p => p.Category.Name),
                _ => sortDescending ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id)
            };
        }
    }
}
