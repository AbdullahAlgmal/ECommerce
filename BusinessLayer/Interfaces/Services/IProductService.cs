
using BusinessLayer.DTOs;
using BusinessLayer.DTOs.Product;
using DataAccessLayer;

namespace BusinessLayer.Interfaces.Services
{
    public interface IProductService : IBaseService<Product, ProductDto, CreateProductDto, UpdateProductDto>
    {
        Task<ProductDto?> GetProductWithDetailsAsync(int id);
        Task<PagedResult<ProductDto>> SearchProductsAsync(ProductFilterDto filter);
        Task<bool> UpdateStockAsync(int productId, int quantity);
        Task<bool> ReduceStockAsync(int productId, int quantity);
        Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int threshold);
        Task<decimal> GetAverageProductPriceAsync();
    }
}
