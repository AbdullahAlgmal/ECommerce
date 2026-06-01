using BusinessLayer.DTOs;
using BusinessLayer.DTOs.Product;
using BusinessLayer.DTOs.Review;
using BusinessLayer.DTOs.User;
using DataAccessLayer;

namespace BusinessLayer.Interfaces.Services
{
    public interface IReviewService : IBaseService<Review, ReviewDto, CreateReviewDto, UpdateReviewDto>
    {
        Task<ReviewDto?> GetReviewWithDetailsAsync(int id);

        Task<IEnumerable<ReviewDto>> GetReviewsByProductAsync(int productId);
        Task<IEnumerable<ReviewDto>> GetReviewsByUserAsync(int userId);
        Task<IEnumerable<ReviewDto>> GetReviewsByRatingRangeAsync(decimal minRating, decimal maxRating);

        Task<ProductRatingDto> GetProductRatingAsync(int productId);
        Task<UserReviewSummaryDto> GetUserReviewSummaryAsync(int userId);
        Task<PagedResult<ReviewDto>> SearchReviewsAsync(ReviewFilterDto filter);

        Task<bool> HasUserReviewedProductAsync(int userId, int productId);

        Task<bool> DeleteReviewsByProductAsync(int productId);
        Task<bool> DeleteReviewsByUserAsync(int userId);
    }
}
