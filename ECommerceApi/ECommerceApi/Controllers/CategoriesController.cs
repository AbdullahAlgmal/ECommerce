using CoreLayer.DTOs.Category;
using CoreLayer.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetAllCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Ok(ApiResponse<IEnumerable<CategoryDto>>.Succ(categories));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<CategoryDto>>.Fail(ex.Message));
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategoryById(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                    return NotFound(ApiResponse<CategoryDto>.Fail($"Category with ID {id} not found"));

                return Ok(ApiResponse<CategoryDto>.Succ(category));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CategoryDto>.Fail(ex.Message));
            }
        }

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

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory([FromBody] CreateCategoryDto createDto)
        {
            try
            {
                var category = await _categoryService.CreateCategoryAsync(createDto);
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

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> UpdateCategory(int id, [FromBody] UpdateCategoryDto updateDto)
        {
            try
            {
                var category = await _categoryService.UpdateCategoryAsync(id, updateDto);
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

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteCategory(int id)
        {
            try
            {
                var deleted = await _categoryService.DeleteCategoryAsync(id);
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
