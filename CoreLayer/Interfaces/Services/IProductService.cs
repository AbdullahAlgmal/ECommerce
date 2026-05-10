using CoreLayer.DTOs;
using CoreLayer.DTOs.Product;

namespace CoreLayer.Interfaces.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<ProductDto?> GetProductWithDetailsAsync(int id);
        Task<ProductDto> CreateProductAsync(CreateProductDto createDto);
        Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateDto);
        Task<bool> DeleteProductAsync(int id);
        Task<PagedResult<ProductDto>> SearchProductsAsync(ProductFilterDto filter);
        Task<bool> UpdateStockAsync(int productId, int quantity);
        Task<bool> ReduceStockAsync(int productId, int quantity);
        Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int threshold);
        Task<decimal> GetAverageProductPriceAsync();
    }
}
