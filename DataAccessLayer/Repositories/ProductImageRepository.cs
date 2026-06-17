using BusinessLayer.Interfaces.Repositories;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class ProductImageRepository : BaseRepository<ProductImage>, IProductImageRepository
    {
        public ProductImageRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<ProductImage>> GetImagesByProductAsync(int productId)
        {
            return await _dbSet
                .Where(pi => pi.ProductId == productId)
                .OrderBy(pi => pi.ImageOrder)
                .ToListAsync();
        }
        public async Task<bool> DeleteImagesByProductAsync(int productId)
        {
            var images = await _dbSet.Where(pi => pi.ProductId == productId).ToListAsync();
            if (!images.Any())
                return false;

            _dbSet.RemoveRange(images);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<ProductImage?> GetPrimaryImageAsync(int productId)
        {
            return await _dbSet
                .Where(pi => pi.ProductId == productId && pi.ImageOrder == 1)
                .FirstOrDefaultAsync();
        }
        public async Task<bool> ReorderImagesAsync(int productId, Dictionary<int, byte> imageOrders)
        {
            var images = await _dbSet.Where(pi => pi.ProductId == productId).ToListAsync();

            foreach (var image in images)
            {
                if (imageOrders.ContainsKey(image.Id))
                {
                    image.ImageOrder = imageOrders[image.Id];
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ProductOwnsImageAsync(int imageId, int productId)
        {
            return await _dbSet
                .AnyAsync(pi => pi.Id == imageId && pi.ProductId == productId);
        }
        public async Task<int> CountImagesByProductAsync(int productId)
        {
            return await _dbSet.CountAsync(pi => pi.ProductId == productId);
        }
    }
}
