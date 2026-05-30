using DataAccessLayer;

namespace BusinessLayer.Interfaces.Repositories
{
    public interface IProductImageRepository : IBaseRepository<ProductImage>
    {
        Task<IEnumerable<ProductImage>> GetImagesByProductAsync(int productId);
        Task<bool> DeleteImagesByProductAsync(int productId);
        Task<ProductImage?> GetPrimaryImageAsync(int productId);
        Task<bool> ReorderImagesAsync(int productId, Dictionary<int, byte> imageOrders);

        Task<bool> UserOwnsAddressAsync(int imageId, int productId);
        Task<int> CountAddressesByUserAsync(int productId);
    }
}
