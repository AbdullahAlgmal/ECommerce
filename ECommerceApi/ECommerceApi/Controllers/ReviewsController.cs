using BusinessLayer.DTOs;
using BusinessLayer.DTOs.Product;
using BusinessLayer.DTOs.Review;
using BusinessLayer.DTOs.User;
using BusinessLayer.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize]
    [EnableRateLimiting("ECommerceLimiter")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly IProductService _productService;
        private readonly IUserService _userService;

        public ReviewsController(
            IReviewService reviewService,
            IProductService productService,
            IUserService userService)
        {
            _reviewService = reviewService;
            _productService = productService;
            _userService = userService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ReviewDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ReviewDto>>>> GetAllReviews()
        {
            try
            {
                var reviews = await _reviewService.GetAllReviewsAsync();
                return Ok(ApiResponse<IEnumerable<ReviewDto>>.Succ(reviews));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<ReviewDto>>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ReviewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<ReviewDto>>> GetReviewById(int id, [FromServices] IAuthorizationService authorizationService)
        {
            try
            {
                var review = await _reviewService.GetReviewByIdAsync(id);
                if (review == null)
                    return NotFound(ApiResponse<ReviewDto>.Fail($"Review with ID {id} not found"));

                var authResult = await authorizationService.AuthorizeAsync(User, id, "AdminOrReviewOwner");
                if (!authResult.Succeeded)
                    return Forbid();

                return Ok(ApiResponse<ReviewDto>.Succ(review));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ReviewDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("{id}/details")]
        [ProducesResponseType(typeof(ApiResponse<ReviewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<ReviewDto>>> GetReviewWithDetails(int id, [FromServices] IAuthorizationService authorizationService)
        {
            try
            {
                var review = await _reviewService.GetReviewWithDetailsAsync(id);
                if (review == null)
                    return NotFound(ApiResponse<ReviewDto>.Fail($"Review with ID {id} not found"));

                var authResult = await authorizationService.AuthorizeAsync(User, id, "AdminOrReviewOwner");
                if (!authResult.Succeeded)
                    return Forbid();

                return Ok(ApiResponse<ReviewDto>.Succ(review));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ReviewDto>.Fail(ex.Message));
            }
        }

        [AllowAnonymous]
        [HttpGet("product/{productId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ReviewDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ReviewDto>>>> GetReviewsByProduct(int productId)
        {
            try
            {
                var productExists = await _productService.GetProductByIdAsync(productId);
                if (productExists == null)
                    return NotFound(ApiResponse<IEnumerable<ReviewDto>>.Fail($"Product with ID {productId} not found"));

                var reviews = await _reviewService.GetReviewsByProductAsync(productId);
                return Ok(ApiResponse<IEnumerable<ReviewDto>>.Succ(reviews));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<ReviewDto>>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ReviewDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ReviewDto>>>> GetReviewsByUser(int userId, [FromServices] IAuthorizationService authorizationService)
        {
            try
            {
                var userExists = await _userService.UserExistsAsync(userId);
                if (!userExists)
                    return NotFound(ApiResponse<IEnumerable<ReviewDto>>.Fail($"User with ID {userId} not found"));
                
                var authResult = await authorizationService.AuthorizeAsync(User, userId, "AdminOrUserOwner");
                if (!authResult.Succeeded)
                    return Forbid();
              
                var reviews = await _reviewService.GetReviewsByUserAsync(userId);
           
                return Ok(ApiResponse<IEnumerable<ReviewDto>>.Succ(reviews));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<ReviewDto>>.Fail(ex.Message));
            }
        }

        [AllowAnonymous]
        [HttpGet("rating-range")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ReviewDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ReviewDto>>>> GetReviewsByRatingRange(
            [FromQuery] decimal minRating,
            [FromQuery] decimal maxRating)
        {
            try
            {
                var reviews = await _reviewService.GetReviewsByRatingRangeAsync(minRating, maxRating);
                return Ok(ApiResponse<IEnumerable<ReviewDto>>.Succ(reviews));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<IEnumerable<ReviewDto>>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<ReviewDto>>.Fail(ex.Message));
            }
        }

        [AllowAnonymous]
        [HttpGet("product/{productId}/rating")]
        [ProducesResponseType(typeof(ApiResponse<ProductRatingDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ProductRatingDto>>> GetProductRating(int productId)
        {
            try
            {
                var productExists = await _productService.GetProductByIdAsync(productId);
                if (productExists == null)
                    return NotFound(ApiResponse<ProductRatingDto>.Fail($"Product with ID {productId} not found"));

                var rating = await _reviewService.GetProductRatingAsync(productId);
                return Ok(ApiResponse<ProductRatingDto>.Succ(rating));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ProductRatingDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("user/{userId}/summary")]
        [ProducesResponseType(typeof(ApiResponse<UserReviewSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<UserReviewSummaryDto>>> GetUserReviewSummary(int userId, [FromServices] IAuthorizationService authorizationService)
        {
            try
            {
                var userExists = await _userService.UserExistsAsync(userId);
                if (!userExists)
                    return NotFound(ApiResponse<UserReviewSummaryDto>.Fail($"User with ID {userId} not found"));

                var summary = await _reviewService.GetUserReviewSummaryAsync(userId);
                return Ok(ApiResponse<UserReviewSummaryDto>.Succ(summary));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserReviewSummaryDto>.Fail(ex.Message));
            }
        }

        [AllowAnonymous]
        [HttpGet("check/{userId}/{productId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> HasUserReviewedProduct(int userId, int productId)
        {
            try
            {
                var hasReviewed = await _reviewService.HasUserReviewedProductAsync(userId, productId);
                return Ok(ApiResponse<bool>.Succ(hasReviewed));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }

        [AllowAnonymous]
        [HttpPost("search")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ReviewDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PagedResult<ReviewDto>>>> SearchReviews([FromBody] ReviewFilterDto filter)
        {
            try
            {
                if (filter.PageNumber < 1)
                    return BadRequest(ApiResponse<PagedResult<ReviewDto>>.Fail("Page number must be greater than 0"));

                if (filter.PageSize < 1 || filter.PageSize > 100)
                    return BadRequest(ApiResponse<PagedResult<ReviewDto>>.Fail("Page size must be between 1 and 100"));

                var result = await _reviewService.SearchReviewsAsync(filter);
                return Ok(ApiResponse<PagedResult<ReviewDto>>.Succ(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PagedResult<ReviewDto>>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ReviewDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<ReviewDto>>> CreateReview([FromBody] CreateReviewDto createDto)
        {
            try
            {
                var review = await _reviewService.CreateReviewAsync(createDto);
                return CreatedAtAction(nameof(GetReviewById), new { id = review.Id },
                    ApiResponse<ReviewDto>.Succ(review, "Review created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("already reviewed"))
                    return Conflict(ApiResponse<ReviewDto>.Fail(ex.Message));
                if (ex.Message.Contains("Product") || ex.Message.Contains("User"))
                    return NotFound(ApiResponse<ReviewDto>.Fail(ex.Message));
                return BadRequest(ApiResponse<ReviewDto>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ReviewDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Customer")]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ReviewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<ReviewDto>>> UpdateReview(int id, [FromBody] UpdateReviewDto updateDto)
        {
            try
            {
                var review = await _reviewService.UpdateReviewAsync(id, updateDto);
                return Ok(ApiResponse<ReviewDto>.Succ(review, "Review updated successfully"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<ReviewDto>.Fail($"Review with ID {id} not found"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ReviewDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Customer")]
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteReview(int id)
        {
            try
            {
                var deleted = await _reviewService.DeleteReviewAsync(id);
                if (!deleted)
                    return NotFound(ApiResponse<bool>.Fail($"Review with ID {id} not found"));

                return Ok(ApiResponse<bool>.Succ(true, "Review deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("product/{productId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteReviewsByProduct(int productId)
        {
            try
            {
                var productExists = await _productService.GetProductByIdAsync(productId);
                if (productExists == null)
                    return NotFound(ApiResponse<bool>.Fail($"Product with ID {productId} not found"));

                var deleted = await _reviewService.DeleteReviewsByProductAsync(productId);
                return Ok(ApiResponse<bool>.Succ(true, $"All reviews for product {productId} deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("user/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteReviewsByUser(int userId)
        {
            try
            {
                var userExists = await _userService.UserExistsAsync(userId);
                if (!userExists)
                    return NotFound(ApiResponse<bool>.Fail($"User with ID {userId} not found"));

                var deleted = await _reviewService.DeleteReviewsByUserAsync(userId);
                return Ok(ApiResponse<bool>.Succ(true, $"All reviews for user {userId} deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }
    }
}
