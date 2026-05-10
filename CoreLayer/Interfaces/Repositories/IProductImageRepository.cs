using DataAccessLayer;

namespace CoreLayer.Interfaces.Repositories
{
    public interface IProductImageRepository
    {
        Task<ProductImage?> GetByIdAsync(int id);
        Task<IEnumerable<ProductImage>> GetAllAsync();
        Task<IEnumerable<ProductImage>> GetImagesByProductAsync(int productId);
        Task<ProductImage> AddAsync(ProductImage entity);
        Task<ProductImage> UpdateAsync(ProductImage entity);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteImagesByProductAsync(int productId);
        Task<ProductImage?> GetPrimaryImageAsync(int productId);
        Task<bool> ReorderImagesAsync(int productId, Dictionary<int, byte> imageOrders);
    }
}
