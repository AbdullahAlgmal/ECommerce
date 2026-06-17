using BusinessLayer.DTOs.Product;
using BusinessLayer.DTOs.ProductImage;
using BusinessLayer.Interfaces.Repositories;
using DataAccessLayer.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccessLayer.Repositories
{
    public class ProductRepository : BaseRepository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext context) : base(context) { }

        public new async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var sql = @"
                SELECT 
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.Quantity,
                p.CategoryId,
                c.Name AS CategoryName,
                (
                    SELECT 
                        pi.Id,
                        pi.Url,
                        pi.ImageOrder,
			            p.Id as ProductId,
			            p.Name as ProductName
                    FROM ProductImages pi
                    WHERE pi.ProductId = p.Id
                    ORDER by pi.ImageOrder ASC
                    FOR JSON PATH
                ) AS ImagesJson,
                (
                    SELECT 
                        AVG(CAST(r.Rating AS FLOAT)) AS AverageRating,
                        COUNT(r.Id) AS ReviewCount
                    FROM Reviews r
                    WHERE r.ProductId = p.Id
                    FOR JSON PATH, WITHOUT_ARRAY_WRAPPER  -- ✅ Add this
                ) AS ReviewsSummaryJson
            FROM Products p
            INNER JOIN Category c ON p.CategoryId = c.Id
            ORDER BY p.Id;";

            var results = await _context.Set<ProductRaw>()
                .FromSqlRaw(sql)
                .ToListAsync();     

            return results.Select(r => new ProductDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Price = r.Price,
                Quantity = r.Quantity,
                CategoryId = r.CategoryId,
                CategoryName = r.CategoryName,
                Images = r.GetImages(),
                AverageRating = r.GetReviewsSummary().AverageRating,
                ReviewCount = r.GetReviewsSummary().ReviewCount
            }).ToList();
        }

        public async Task<ProductDto?> GetProductWithDetailsAsync(int id)
        {
            var sql = @"
                SELECT 
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.Quantity,
                p.CategoryId,
                c.Name AS CategoryName,
                (
                    SELECT 
                        pi.Id,
                        pi.Url,
                        pi.ImageOrder,
			            p.Id as ProductId,
			            p.Name as ProductName
                    FROM ProductImages pi
                    WHERE pi.ProductId = p.Id
                    ORDER by pi.ImageOrder ASC
                    FOR JSON PATH
                ) AS ImagesJson,
                (
                    SELECT 
                        AVG(CAST(r.Rating AS FLOAT)) AS AverageRating,
                        COUNT(r.Id) AS ReviewCount
                    FROM Reviews r
                    WHERE r.ProductId = p.Id
                    FOR JSON PATH, WITHOUT_ARRAY_WRAPPER  -- ✅ Add this
                ) AS ReviewsSummaryJson
            FROM Products p
            INNER JOIN Category c ON p.CategoryId = c.Id
            WHERE p.Id = @productId";

            var result = await _context.Set<ProductRaw>()
                .FromSqlRaw(sql, new SqlParameter("@productId", id)).FirstOrDefaultAsync();

            if (result == null)
                return null;

            var productDto = new ProductDto
            {
                Id = result.Id,
                Name = result.Name,
                Description = result.Description,
                Price = result.Price,
                Quantity = result.Quantity,
                CategoryId = result.CategoryId,
                CategoryName = result.CategoryName,
                Images = result.GetImages(),
                AverageRating = result.GetReviewsSummary().AverageRating,
                ReviewCount = result.GetReviewsSummary().ReviewCount
            };
            return productDto;
        }
        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId)
        {
            var sql = @"
                SELECT 
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.Quantity,
                p.CategoryId,
                c.Name AS CategoryName,
                (
                    SELECT 
                        pi.Id,
                        pi.Url,
                        pi.ImageOrder,
			            p.Id as ProductId,
			            p.Name as ProductName
                    FROM ProductImages pi
                    WHERE pi.ProductId = p.Id
                    ORDER by pi.ImageOrder ASC
                    FOR JSON PATH
                ) AS ImagesJson,
                (
                    SELECT 
                        AVG(CAST(r.Rating AS FLOAT)) AS AverageRating,
                        COUNT(r.Id) AS ReviewCount
                    FROM Reviews r
                    WHERE r.ProductId = p.Id
                    FOR JSON PATH, WITHOUT_ARRAY_WRAPPER  -- ✅ Add this
                ) AS ReviewsSummaryJson
            FROM Products p           
            INNER JOIN Category c ON p.CategoryId = c.Id
            where p.CategoryId = @categoryId
            ORDER BY p.Id;";

            var results = await _context.Set<ProductRaw>()
                .FromSqlRaw(sql, new SqlParameter("@categoryId", categoryId))
                .ToListAsync();

            return results.Select(r => new ProductDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Price = r.Price,
                Quantity = r.Quantity,
                CategoryId = r.CategoryId,
                CategoryName = r.CategoryName,
                Images = r.GetImages(),
                AverageRating = r.GetReviewsSummary().AverageRating,
                ReviewCount = r.GetReviewsSummary().ReviewCount
            }).ToList();
        }
        public async Task<IEnumerable<ProductDto>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            return await _dbSet
                .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
                .SelectMany(p => p.ProductImages, (p, pi) => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    Images = p.ProductImages.Select(pi => new ProductImageDto
                    {
                        Id = pi.Id,
                        Url = pi.Url,
                        ImageOrder = pi.ImageOrder,
                        ProductId = p.Id,
                        ProductName = p.Name
                    }).ToList(),
                    AverageRating = (double)p.Reviews.Average(r => r.Rating),
                    ReviewCount = p.Reviews.Count()
                })
                .OrderBy(p => p.Price)
                .ToListAsync();
        }
        public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int threshold)
        {
            return await _dbSet
                .Where(p => p.Quantity <= threshold)
                .SelectMany(p => p.ProductImages, (p, pi) => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    Images = p.ProductImages.Select(pi => new ProductImageDto
                    {
                        Id = pi.Id,
                        Url = pi.Url,
                        ImageOrder = pi.ImageOrder,
                        ProductId = p.Id,
                        ProductName = p.Name
                    }).ToList(),
                    AverageRating = (double)p.Reviews.Average(r => r.Rating),
                    ReviewCount = p.Reviews.Count()
                })
                .OrderBy(p => p.Quantity)
                .ToListAsync();
        }
        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
        {
            var term = searchTerm.ToLower();
            return await _dbSet
                .Where(p => p.Name.ToLower().Contains(term) || p.Description.ToLower().Contains(term))
                .SelectMany(p => p.ProductImages, (p, pi) => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    Images = p.ProductImages.Select(pi => new ProductImageDto
                    {
                        Id = pi.Id,
                        Url = pi.Url,
                        ImageOrder = pi.ImageOrder,
                        ProductId = p.Id,
                        ProductName = p.Name
                    }).ToList(),
                    AverageRating = (double)p.Reviews.Average(r => r.Rating),
                    ReviewCount = p.Reviews.Count()
                })
                .ToListAsync();
        }

        public async Task<bool> IsProductNameUniqueAsync(string name, int? excludeId = null)
        {
            var query = _dbSet.Where(p => p.Name.ToLower() == name.ToLower());

            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);

            return !await query.AnyAsync();
        }

        public async Task<decimal> GetAverageProductPriceAsync()
        {
            return await _dbSet.AverageAsync(p => p.Price);
        }
        public async Task<(IEnumerable<ProductDto> Products, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<Product, bool>>? predicate = null,
            string? sortBy = null,
            bool sortDescending = false)
        {
            var query = _dbSet.AsQueryable();

            if (predicate != null)
                query = query.Where(predicate);

            var totalCount = await query.CountAsync();

            query = ApplySorting(query, sortBy, sortDescending);

            var products = await query.
                SelectMany(p => p.ProductImages, (p, pi) => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    Images = p.ProductImages.Select(pi => new ProductImageDto
                    {
                        Id = pi.Id,
                        Url = pi.Url,
                        ImageOrder = pi.ImageOrder,
                        ProductId = p.Id,
                        ProductName = p.Name
                    }).ToList(),
                    AverageRating = (double)p.Reviews.Average(r => r.Rating),
                    ReviewCount = p.Reviews.Count()
                })
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalCount);
        }

        public async Task<bool> UpdateStockAsync(int productId, int quantity)
        {
            var product = await GetByIdAsync(productId);
            if (product == null)
                return false;

            product.Quantity = quantity;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> ReduceStockAsync(int productId, int quantity)
        {
            var product = await GetByIdAsync(productId);
            if (product == null || product.Quantity < quantity)
                return false;

            product.Quantity -= quantity;
            await _context.SaveChangesAsync();
            return true;
        }

        private IQueryable<Product> ApplySorting(IQueryable<Product> query, string? sortBy, bool sortDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(p => p.Id);

            return sortBy.ToLower() switch
            {
                "name" => sortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "price" => sortDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                "quantity" => sortDescending ? query.OrderByDescending(p => p.Quantity) : query.OrderBy(p => p.Quantity),
                "category" => sortDescending ? query.OrderByDescending(p => p.Category.Name) : query.OrderBy(p => p.Category.Name),
                _ => sortDescending ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id)
            };
        }
    }
}
