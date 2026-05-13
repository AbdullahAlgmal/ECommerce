using CoreLayer.DTOs;
using CoreLayer.DTOs.Product;
using CoreLayer.DTOs.Review;
using CoreLayer.DTOs.User;

namespace CoreLayer.Interfaces.Services
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDto>> GetAllReviewsAsync();
        Task<ReviewDto?> GetReviewByIdAsync(int id);
        Task<ReviewDto?> GetReviewWithDetailsAsync(int id);
        Task<ReviewDto> CreateReviewAsync(CreateReviewDto createDto);
        Task<ReviewDto> UpdateReviewAsync(int id, UpdateReviewDto updateDto);
        Task<bool> DeleteReviewAsync(int id);

        Task<IEnumerable<ReviewDto>> GetReviewsByProductAsync(int productId);
        Task<IEnumerable<ReviewDto>> GetReviewsByUserAsync(int userId);
        Task<IEnumerable<ReviewDto>> GetReviewsByRatingRangeAsync(decimal minRating, decimal maxRating);

        Task<ProductRatingDto> GetProductRatingAsync(int productId);
        Task<UserReviewSummaryDto> GetUserReviewSummaryAsync(int userId);
        Task<PagedResult<ReviewDto>> SearchReviewsAsync(ReviewFilterDto filter);

        Task<bool> HasUserReviewedProductAsync(int userId, int productId);
        Task<bool> ReviewExistsAsync(int id);

        Task<bool> DeleteReviewsByProductAsync(int productId);
        Task<bool> DeleteReviewsByUserAsync(int userId);
    }
}
