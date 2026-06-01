using BusinessLayer.DTOs.ProductImage;
using DataAccessLayer;

namespace BusinessLayer.Interfaces.Services
{
    public interface IProductImageService : IBaseService<ProductImage, ProductImageDto, CreateProductImageDto, UpdateProductImageDto>
    {
        Task<IEnumerable<ProductImageDto>> GetImagesByProductAsync(int productId);
        Task<ProductImageDto?> GetPrimaryImageAsync(int productId);
        Task<ProductImageDto> SetAsPrimaryAsync(int imageId, int productId);

        Task<IEnumerable<ProductImageDto>> BulkUploadImagesAsync(BulkUploadImagesDto bulkDto);
        Task<bool> DeleteAllProductImagesAsync(int productId);
        Task<bool> ReorderImagesAsync(int productId, Dictionary<int, byte> imageOrders);

        Task<bool> ImageBelongsToProductAsync(int imageId, int productId);
        Task<int> GetImageCountByProductAsync(int productId);
    }
}
