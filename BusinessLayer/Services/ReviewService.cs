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
    public class ReviewService : BaseService<Review, ReviewDto, CreateReviewDto, UpdateReviewDto>, IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;

        public ReviewService(
            IReviewRepository reviewRepository,
            IProductRepository productRepository,
            IUserRepository userRepository) : base(reviewRepository)
        {
            _reviewRepository = reviewRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
        }

        public async Task<ReviewDto?> GetReviewWithDetailsAsync(int id)
        {
            var review = await _reviewRepository.GetReviewWithDetailsAsync(id);
            return review;
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByProductAsync(int productId)
        {
            var productExists = await _productRepository.ExistsAsync(p => p.Id == productId);
            if (!productExists)
                throw new InvalidOperationException($"Product with ID {productId} does not exist");

            var reviews = await _reviewRepository.GetReviewsByProductAsync(productId);
            return reviews;
        }
        public async Task<IEnumerable<ReviewDto>> GetReviewsByUserAsync(int userId)
        {
            var userExists = await _userRepository.ExistsAsync(u => u.Id == userId);
            if (!userExists)
                throw new InvalidOperationException($"User with ID {userId} does not exist");

            var reviews = await _reviewRepository.GetReviewsByUserAsync(userId);
            return reviews;
        }
        public async Task<IEnumerable<ReviewDto>> GetReviewsByRatingRangeAsync(decimal minRating, decimal maxRating)
        {
            if (minRating < 0 || minRating > 5 || maxRating < 0 || maxRating > 5 || minRating > maxRating)
                throw new InvalidOperationException("Invalid rating range. Ratings must be between 0 and 5");

            var reviews = await _reviewRepository.GetReviewsByRatingAsync(minRating, maxRating);
            return reviews;
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

            var averageRating = reviewsList.Count != 0 ? reviewsList.Average(r => r.Rating) : 0;
            var ratingDistribution = await _reviewRepository.GetRatingDistributionForUserAsync(userId);
            var recentReviews = 
                reviewsList
                .Take(5)
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
                Items = reviews,
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

        private static Expression<Func<Review, bool>>? BuildPredicate(ReviewFilterDto filter)
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
        private static Expression<Func<Review, bool>>? CombinePredicates(
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

        protected override Task<ReviewDto> MapToDto(Review entity)
        {
            return Task.FromResult(new ReviewDto
            {
                Id = entity.Id,
                ReviewText = entity.ReviewText,
                Rating = entity.Rating,
                ReviewDate = entity.ReviewDate,
                ProductId = entity.ProductId,
                ProductName = entity.Product?.Name ?? "Unknown",
                UserId = entity.UserId,
                UserName = entity.User != null ? $"{entity.User.FirstName} {entity.User.LastName}" : "Unknown",
                UserEmail = entity.User?.Email ?? "Unknown"
            });
        }
        protected override async Task<IEnumerable<ReviewDto>> MapToDtoList(IEnumerable<Review> entities)
        {
            var dtoList = entities.Select(MapToDto);
            return await Task.WhenAll(dtoList);
        }
        protected override Review MapToEntity(CreateReviewDto createDto)
        {
            return new Review
            {
                ReviewText = createDto.ReviewText.Trim(),
                Rating = createDto.Rating,
                ReviewDate = DateOnly.FromDateTime(DateTime.Today),
                ProductId = createDto.ProductId,
                UserId = createDto.UserId
            };
        }
        protected override void UpdateEntity(Review entity, UpdateReviewDto updateDto)
        {
            entity.ReviewText = updateDto.ReviewText.Trim();
            entity.Rating = updateDto.Rating;
            entity.ReviewDate = DateOnly.FromDateTime(DateTime.Today);
        }
        protected override async Task ValidateBeforeCreateAsync(CreateReviewDto createDto)
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

        }
        protected override async Task ValidateBeforeUpdateAsync(int id, UpdateReviewDto updateDto)
        {
            _ = await _reviewRepository.GetReviewWithDetailsAsync(id) ?? throw new KeyNotFoundException($"Review with ID {id} not found");
        }
    }
}
