using CoreLayer.Interfaces.Repositories;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories
{
    public class ProductImageRepository : IProductImageRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<ProductImage> _dbSet;

        public ProductImageRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<ProductImage>();
        }

        public async Task<ProductImage?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(pi => pi.Product)
                .FirstOrDefaultAsync(pi => pi.Id == id);
        }
        public async Task<IEnumerable<ProductImage>> GetAllAsync()
        {
            return await _dbSet
                .Include(pi => pi.Product)
                .OrderBy(pi => pi.ImageOrder)
                .ToListAsync();
        }
        public async Task<IEnumerable<ProductImage>> GetImagesByProductAsync(int productId)
        {
            return await _dbSet
                .Where(pi => pi.ProductId == productId)
                .OrderBy(pi => pi.ImageOrder)
                .ToListAsync();
        }
        public async Task<ProductImage> AddAsync(ProductImage entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task<ProductImage> UpdateAsync(ProductImage entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var image = await GetByIdAsync(id);
            if (image == null)
                return false;

            _dbSet.Remove(image);
            await _context.SaveChangesAsync();
            return true;
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
                .Where(pi => pi.ProductId == productId)
                .OrderBy(pi => pi.ImageOrder)
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
    }
}
