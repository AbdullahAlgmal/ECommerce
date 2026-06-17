using BusinessLayer.DTOs.Review;
using BusinessLayer.Interfaces.Repositories;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccessLayer.Repositories
{
    public class ReviewRepository : BaseRepository<Review>, IReviewRepository
    {
        public ReviewRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<ReviewDto?> GetReviewWithDetailsAsync(int id)
        {
            return await _dbSet.Where(r => r.Id == id).Select(r => new ReviewDto
            {
                Id = r.Id,
                ReviewText = r.ReviewText,
                Rating = r.Rating,
                ReviewDate = r.ReviewDate,
                ProductId = r.ProductId,
                ProductName = r.Product.Name ?? "Unknown",
                UserId = r.UserId,
                UserName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : "Unknown",
                UserEmail = r.User!.Email ?? "Unknown"
            }).FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<ReviewDto>> GetReviewsByProductAsync(int productId)
        {
            return await _dbSet.Where(r => r.ProductId == productId).Select(r => new ReviewDto
            {
                Id = r.Id,
                ReviewText = r.ReviewText,
                Rating = r.Rating,
                ReviewDate = r.ReviewDate,
                ProductId = r.ProductId,
                ProductName = r.Product.Name ?? "Unknown",
                UserId = r.UserId,
                UserName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : "Unknown",
                UserEmail = r.User!.Email ?? "Unknown"
            }).OrderByDescending(r => r.ReviewDate).ToListAsync();
        }
        public async Task<IEnumerable<ReviewDto>> GetReviewsByUserAsync(int userId)
        {
            return await _dbSet
                .Where(r => r.UserId == userId)            
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    ReviewText = r.ReviewText,
                    Rating = r.Rating,
                    ReviewDate = r.ReviewDate,
                    ProductId = r.ProductId,
                    ProductName = r.Product.Name ?? "Unknown",
                    UserId = r.UserId,
                    UserName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : "Unknown",
                    UserEmail = r.User!.Email ?? "Unknown"
                })
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();
        }
        public async Task<IEnumerable<ReviewDto>> GetReviewsByRatingAsync(decimal minRating, decimal maxRating)
        {
            return await _dbSet
                .Where(r => r.Rating >= minRating && r.Rating <= maxRating)             
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    ReviewText = r.ReviewText,
                    Rating = r.Rating,
                    ReviewDate = r.ReviewDate,
                    ProductId = r.ProductId,
                    ProductName = r.Product.Name ?? "Unknown",
                    UserId = r.UserId,
                    UserName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : "Unknown",
                    UserEmail = r.User!.Email ?? "Unknown"
                })
                .OrderByDescending(r => r.Rating)
                .ToListAsync();
        }

        public async Task<bool> HasUserReviewedProductAsync(int userId, int productId)
        {
            return await _dbSet.AnyAsync(r => r.UserId == userId && r.ProductId == productId);
        }

        public async Task<decimal> GetAverageRatingForProductAsync(int productId)
        {
            return await _dbSet
                .Where(r => r.ProductId == productId)
                .AverageAsync(r => r.Rating);
        }
        public async Task<Dictionary<int, int>> GetRatingDistributionForProductAsync(int productId)
        {
            return await _dbSet
                .Where(r => r.ProductId == productId)
                .GroupBy(r => (int)Math.Floor(r.Rating))
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Rating, g => g.Count);
        }
        public async Task<Dictionary<int, int>> GetRatingDistributionForUserAsync(int userId)
        {
            return await _dbSet
                .Where(r => r.UserId == userId)
                .GroupBy(r => (int)Math.Floor(r.Rating))
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Rating, g => g.Count);
        }

        public async Task<(IEnumerable<ReviewDto> Reviews, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<Review, bool>>? predicate = null,
            string? sortBy = null,
            bool sortDescending = false)
        {
            var query = _dbSet               
                .AsQueryable();

            if (predicate != null)
                query = query.Where(predicate);

            var totalCount = await query.CountAsync();

            query = ApplySorting(query, sortBy, sortDescending);

            var reviews = await query
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    ReviewText = r.ReviewText,
                    Rating = r.Rating,
                    ReviewDate = r.ReviewDate,
                    ProductId = r.ProductId,
                    ProductName = r.Product.Name ?? "Unknown",
                    UserId = r.UserId,
                    UserName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : "Unknown",
                    UserEmail = r.User!.Email ?? "Unknown"
                })
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (reviews, totalCount);
        }

        public async Task<bool> DeleteReviewsByProductAsync(int productId)
        {
            var reviews = await _dbSet.Where(r => r.ProductId == productId).ToListAsync();
            if (!reviews.Any())
                return false;

            _dbSet.RemoveRange(reviews);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteReviewsByUserAsync(int userId)
        {
            var reviews = await _dbSet.Where(r => r.UserId == userId).ToListAsync();
            if (!reviews.Any())
                return false;

            _dbSet.RemoveRange(reviews);
            await _context.SaveChangesAsync();
            return true;
        }

        private IQueryable<Review> ApplySorting(IQueryable<Review> query, string? sortBy, bool sortDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return sortDescending ? query.OrderByDescending(r => r.ReviewDate) : query.OrderBy(r => r.ReviewDate);

            return sortBy.ToLower() switch
            {
                "rating" => sortDescending ? query.OrderByDescending(r => r.Rating) : query.OrderBy(r => r.Rating),
                "reviewdate" => sortDescending ? query.OrderByDescending(r => r.ReviewDate) : query.OrderBy(r => r.ReviewDate),
                "product" => sortDescending ? query.OrderByDescending(r => r.Product.Name) : query.OrderBy(r => r.Product.Name),
                "user" => sortDescending ? query.OrderByDescending(r => r.User.FirstName) : query.OrderBy(r => r.User.FirstName),
                _ => sortDescending ? query.OrderByDescending(r => r.ReviewDate) : query.OrderBy(r => r.ReviewDate)
            };
        }
    }
}
