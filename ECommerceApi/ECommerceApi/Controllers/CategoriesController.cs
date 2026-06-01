using BusinessLayer.DTOs.Category;
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
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetAllCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllAsync();
                return Ok(ApiResponse<IEnumerable<CategoryDto>>.Succ(categories));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<CategoryDto>>.Fail(ex.Message));
            }
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategoryById(int id)
        {
            try
            {
                var category = await _categoryService.GetByIdAsync(id);
                if (category == null)
                    return NotFound(ApiResponse<CategoryDto>.Fail($"Category with ID {id} not found"));

                return Ok(ApiResponse<CategoryDto>.Succ(category));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CategoryDto>.Fail(ex.Message));
            }
        }

        [AllowAnonymous]
        [HttpGet("{id}/products")]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategoryWithProducts(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryWithProductsAsync(id);
                if (category == null)
                    return NotFound(ApiResponse<CategoryDto>.Fail($"Category with ID {id} not found"));

                return Ok(ApiResponse<CategoryDto>.Succ(category));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CategoryDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory([FromBody] CreateCategoryDto createDto)
        {
            try
            {
                var category = await _categoryService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id },
                    ApiResponse<CategoryDto>.Succ(category, "Category created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<CategoryDto>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CategoryDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> UpdateCategory(int id, [FromBody] UpdateCategoryDto updateDto)
        {
            try
            {
                var category = await _categoryService.UpdateAsync(id, updateDto);
                return Ok(ApiResponse<CategoryDto>.Succ(category, "Category updated successfully"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<CategoryDto>.Fail($"Category with ID {id} not found"));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<CategoryDto>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CategoryDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteCategory(int id)
        {
            try
            {
                var deleted = await _categoryService.DeleteAsync(id);
                if (!deleted)
                    return NotFound(ApiResponse<bool>.Fail($"Category with ID {id} not found"));

                return Ok(ApiResponse<bool>.Succ(true, "Category deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }
    }
}
