using BusinessLayer.DTOs;
using BusinessLayer.DTOs.Product;
using BusinessLayer.DTOs.Review;
using BusinessLayer.DTOs.User;
using BusinessLayer.Interfaces.Repositories;
using BusinessLayer.Interfaces.Services;
using DataAccessLayer;
using System.Linq.Expressions;

namespace BusinessLayer.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;

        public ReviewService(
            IReviewRepository reviewRepository,
            IProductRepository productRepository,
            IUserRepository userRepository)
        {
            _reviewRepository = reviewRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<ReviewDto>> GetAllReviewsAsync()
        {
            var reviews = await _reviewRepository.GetAllAsync();
            return reviews.Select(MapToReviewDto);
        }

        public async Task<ReviewDto?> GetReviewByIdAsync(int id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);
            return review != null ? MapToReviewDto(review) : null;
        }
        public async Task<ReviewDto?> GetReviewWithDetailsAsync(int id)
        {
            var review = await _reviewRepository.GetReviewWithDetailsAsync(id);
            return review != null ? MapToReviewDto(review) : null;
        }
        public async Task<ReviewDto> CreateReviewAsync(CreateReviewDto createDto)
        {
            var productExists = await _productRepository.ExistsAsync(p => p.Id == createDto.ProductId);
            if (!productExists)
                throw new InvalidOperationException($"Product with ID {createDto.ProductId} does not exist");

            var userExists = await _userRepository.ExistsAsync(u => u.Id == createDto.UserId);
            if (!userExists)
                throw new InvalidOperationException($"User with ID {createDto.UserId} does not exist");

            var hasReviewed = await _reviewRepository.HasUserReviewedProductAsync(createDto.UserId, createDto.ProductId);
            if (hasReviewed)
                throw new InvalidOperationException("User has already reviewed this product");

            var review = new Review
            {
                ReviewText = createDto.ReviewText.Trim(),
                Rating = createDto.Rating,
                ReviewDate = DateOnly.FromDateTime(DateTime.Today),
                ProductId = createDto.ProductId,
                UserId = createDto.UserId
            };

            var createdReview = await _reviewRepository.AddAsync(review);
            return MapToReviewDto(createdReview);
        }
        public async Task<ReviewDto> UpdateReviewAsync(int id, UpdateReviewDto updateDto)
        {
            var review = await _reviewRepository.GetReviewWithDetailsAsync(id);
            if (review == null)
                throw new KeyNotFoundException($"Review with ID {id} not found");

            review.ReviewText = updateDto.ReviewText.Trim();
            review.Rating = updateDto.Rating;

            var updatedReview = await _reviewRepository.UpdateAsync(review);
            return MapToReviewDto(updatedReview);
        }
        public async Task<bool> DeleteReviewAsync(int id)
        {
            return await _reviewRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByProductAsync(int productId)
        {
            var productExists = await _productRepository.ExistsAsync(p => p.Id == productId);
            if (!productExists)
                throw new InvalidOperationException($"Product with ID {productId} does not exist");

            var reviews = await _reviewRepository.GetReviewsByProductAsync(productId);
            return reviews.Select(MapToReviewDto);
        }
        public async Task<IEnumerable<ReviewDto>> GetReviewsByUserAsync(int userId)
        {
            var userExists = await _userRepository.ExistsAsync(u => u.Id == userId);
            if (!userExists)
                throw new InvalidOperationException($"User with ID {userId} does not exist");

            var reviews = await _reviewRepository.GetReviewsByUserAsync(userId);
            return reviews.Select(MapToReviewDto);
        }
        public async Task<IEnumerable<ReviewDto>> GetReviewsByRatingRangeAsync(decimal minRating, decimal maxRating)
        {
            if (minRating < 0 || minRating > 5 || maxRating < 0 || maxRating > 5 || minRating > maxRating)
                throw new InvalidOperationException("Invalid rating range. Ratings must be between 0 and 5");

            var reviews = await _reviewRepository.GetReviewsByRatingAsync(minRating, maxRating);
            return reviews.Select(MapToReviewDto);
        }

        public async Task<ProductRatingDto> GetProductRatingAsync(int productId)
        {
            var productExists = await _productRepository.ExistsAsync(p => p.Id == productId);
            if (!productExists)
                throw new InvalidOperationException($"Product with ID {productId} does not exist");

            var product = await _productRepository.GetByIdAsync(productId);
            var averageRating = await _reviewRepository.GetAverageRatingForProductAsync(productId);
            var totalReviews = await _reviewRepository.CountAsync(r => r.ProductId == productId);
            var ratingDistribution = await _reviewRepository.GetRatingDistributionForProductAsync(productId);

            return new ProductRatingDto
            {
                ProductId = productId,
                ProductName = product?.Name ?? "Unknown",
                AverageRating = averageRating,
                TotalReviews = totalReviews,
                RatingDistribution = ratingDistribution
            };
        }
        public async Task<UserReviewSummaryDto> GetUserReviewSummaryAsync(int userId)
        {
            var userExists = await _userRepository.ExistsAsync(u => u.Id == userId);
            if (!userExists)
                throw new InvalidOperationException($"User with ID {userId} does not exist");

            var user = await _userRepository.GetByIdAsync(userId);
            var reviews = await _reviewRepository.GetReviewsByUserAsync(userId);
            var reviewsList = reviews.ToList();

            var averageRating = reviewsList.Any() ? reviewsList.Average(r => r.Rating) : 0;
            var ratingDistribution = await _reviewRepository.GetRatingDistributionForUserAsync(userId);
            var recentReviews = reviewsList
                .OrderByDescending(r => r.ReviewDate)
                .Take(5)
                .Select(MapToReviewDto)
                .ToList();

            return new UserReviewSummaryDto
            {
                UserId = userId,
                UserName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown",
                TotalReviews = reviewsList.Count,
                AverageRating = averageRating,
                RatingDistribution = ratingDistribution,
                RecentReviews = recentReviews
            };
        }
        public async Task<PagedResult<ReviewDto>> SearchReviewsAsync(ReviewFilterDto filter)
        {
            var predicate = BuildPredicate(filter);

            var (reviews, totalCount) = await _reviewRepository.GetPagedAsync(
                filter.PageNumber,
                filter.PageSize,
                predicate,
                filter.SortBy,
                filter.SortDescending);

            return new PagedResult<ReviewDto>
            {
                Items = reviews.Select(MapToReviewDto),
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<bool> HasUserReviewedProductAsync(int userId, int productId)
        {
            return await _reviewRepository.HasUserReviewedProductAsync(userId, productId);
        }
        public async Task<bool> ReviewExistsAsync(int id)
        {
            return await _reviewRepository.ExistsAsync(r => r.Id == id);
        }

        public async Task<bool> DeleteReviewsByProductAsync(int productId)
        {
            var productExists = await _productRepository.ExistsAsync(p => p.Id == productId);
            if (!productExists)
                throw new InvalidOperationException($"Product with ID {productId} does not exist");

            return await _reviewRepository.DeleteReviewsByProductAsync(productId);
        }
        public async Task<bool> DeleteReviewsByUserAsync(int userId)
        {
            var userExists = await _userRepository.ExistsAsync(u => u.Id == userId);
            if (!userExists)
                throw new InvalidOperationException($"User with ID {userId} does not exist");

            return await _reviewRepository.DeleteReviewsByUserAsync(userId);
        }

        private Expression<Func<Review, bool>>? BuildPredicate(ReviewFilterDto filter)
        {
            Expression<Func<Review, bool>>? predicate = null;

            if (filter.ProductId.HasValue)
            {
                predicate = CombinePredicates(predicate, r => r.ProductId == filter.ProductId.Value);
            }

            if (filter.UserId.HasValue)
            {
                predicate = CombinePredicates(predicate, r => r.UserId == filter.UserId.Value);
            }

            if (filter.MinRating.HasValue)
            {
                predicate = CombinePredicates(predicate, r => r.Rating >= filter.MinRating.Value);
            }

            if (filter.MaxRating.HasValue)
            {
                predicate = CombinePredicates(predicate, r => r.Rating <= filter.MaxRating.Value);
            }

            if (filter.ReviewDateFrom.HasValue)
            {
                predicate = CombinePredicates(predicate, r => r.ReviewDate >= filter.ReviewDateFrom.Value);
            }

            if (filter.ReviewDateTo.HasValue)
            {
                predicate = CombinePredicates(predicate, r => r.ReviewDate <= filter.ReviewDateTo.Value);
            }

            return predicate;
        }
        private Expression<Func<Review, bool>>? CombinePredicates(
            Expression<Func<Review, bool>>? existing,
            Expression<Func<Review, bool>> newPredicate)
        {
            if (existing == null)
                return newPredicate;

            var parameter = Expression.Parameter(typeof(Review));
            var combined = Expression.AndAlso(
                Expression.Invoke(existing, parameter),
                Expression.Invoke(newPredicate, parameter));

            return Expression.Lambda<Func<Review, bool>>(combined, parameter);
        }
        private ReviewDto MapToReviewDto(Review review)
        {
            return new ReviewDto
            {
                Id = review.Id,
                ReviewText = review.ReviewText,
                Rating = review.Rating,
                ReviewDate = review.ReviewDate,
                ProductId = review.ProductId,
                ProductName = review.Product?.Name ?? "Unknown",
                UserId = review.UserId,
                UserName = review.User != null ? $"{review.User.FirstName} {review.User.LastName}" : "Unknown",
                UserEmail = review.User?.Email ?? "Unknown"
            };
        }
    }
}
