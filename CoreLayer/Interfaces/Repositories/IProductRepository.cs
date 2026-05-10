using DataAccessLayer;
using System.Linq.Expressions;

namespace CoreLayer.Interfaces.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> AddAsync(Product entity);
        Task<Product> UpdateAsync(Product entity);
        Task<bool> DeleteAsync(int id);

        Task<IEnumerable<Product>> FindAsync(Expression<Func<Product, bool>> predicate);
        Task<Product?> GetProductWithDetailsAsync(int id);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold);
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);

        Task<bool> ExistsAsync(Expression<Func<Product, bool>> predicate);
        Task<bool> IsProductNameUniqueAsync(string name, int? excludeId = null);

        Task<int> CountAsync(Expression<Func<Product, bool>>? predicate = null);
        Task<decimal> GetAverageProductPriceAsync();
        Task<(IEnumerable<Product> Products, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<Product, bool>>? predicate = null,
            string? sortBy = null,
            bool sortDescending = false);

        Task<bool> UpdateStockAsync(int productId, int quantity);
        Task<bool> ReduceStockAsync(int productId, int quantity);
    }
}
