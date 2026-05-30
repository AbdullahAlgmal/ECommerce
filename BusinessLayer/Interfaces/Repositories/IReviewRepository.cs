using DataAccessLayer;
using System.Linq.Expressions;

namespace BusinessLayer.Interfaces.Repositories
{
    public interface IReviewRepository : IBaseRepository<Review>
    {
        Task<Review?> GetReviewWithDetailsAsync(int id);
        Task<IEnumerable<Review>> GetReviewsByProductAsync(int productId);
        Task<IEnumerable<Review>> GetReviewsByUserAsync(int userId);
        Task<IEnumerable<Review>> GetReviewsByRatingAsync(decimal minRating, decimal maxRating);

        Task<bool> HasUserReviewedProductAsync(int userId, int productId);

        Task<decimal> GetAverageRatingForProductAsync(int productId);
        Task<Dictionary<int, int>> GetRatingDistributionForProductAsync(int productId);
        Task<Dictionary<int, int>> GetRatingDistributionForUserAsync(int userId);

        Task<(IEnumerable<Review> Reviews, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<Review, bool>>? predicate = null,
            string? sortBy = null,
            bool sortDescending = false);

        Task<bool> DeleteReviewsByProductAsync(int productId);
        Task<bool> DeleteReviewsByUserAsync(int userId);
    }
}
