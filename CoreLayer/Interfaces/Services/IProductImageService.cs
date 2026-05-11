using CoreLayer.DTOs.ProductImage;

namespace CoreLayer.Interfaces.Services
{
    public interface IProductImageService
    {
        Task<IEnumerable<ProductImageDto>> GetAllImagesAsync();
        Task<ProductImageDto?> GetImageByIdAsync(int id);
        Task<ProductImageDto> CreateImageAsync(CreateProductImageDto createDto);
        Task<ProductImageDto> UpdateImageAsync(int id, UpdateProductImageDto updateDto);
        Task<bool> DeleteImageAsync(int id);

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
