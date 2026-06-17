using BusinessLayer.DTOs.Product;
using DataAccessLayer;
using System.Linq.Expressions;

namespace BusinessLayer.Interfaces.Repositories
{
    public interface IProductRepository : IBaseRepository<Product>
    {
        new Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<ProductDto?> GetProductWithDetailsAsync(int id);
        Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<ProductDto>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int threshold);
        Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm);

        Task<bool> IsProductNameUniqueAsync(string name, int? excludeId = null);

        Task<decimal> GetAverageProductPriceAsync();
        Task<(IEnumerable<ProductDto> Products, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<Product, bool>>? predicate = null,
            string? sortBy = null,
            bool sortDescending = false);

        Task<bool> UpdateStockAsync(int productId, int quantity);
        Task<bool> ReduceStockAsync(int productId, int quantity);
    }
}
