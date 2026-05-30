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

        public async Task<Review?> GetReviewWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(r => r.Product)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
        public async Task<IEnumerable<Review>> GetReviewsByProductAsync(int productId)
        {
            return await _dbSet
                .Include(r => r.User)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();
        }
        public async Task<IEnumerable<Review>> GetReviewsByUserAsync(int userId)
        {
            return await _dbSet
                .Include(r => r.Product)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();
        }
        public async Task<IEnumerable<Review>> GetReviewsByRatingAsync(decimal minRating, decimal maxRating)
        {
            return await _dbSet
                .Include(r => r.Product)
                .Include(r => r.User)
                .Where(r => r.Rating >= minRating && r.Rating <= maxRating)
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

        public async Task<(IEnumerable<Review> Reviews, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<Review, bool>>? predicate = null,
            string? sortBy = null,
            bool sortDescending = false)
        {
            var query = _dbSet
                .Include(r => r.Product)
                .Include(r => r.User)
                .AsQueryable();

            if (predicate != null)
                query = query.Where(predicate);

            var totalCount = await query.CountAsync();

            query = ApplySorting(query, sortBy, sortDescending);

            var reviews = await query
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
