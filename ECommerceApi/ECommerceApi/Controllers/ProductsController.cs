using CoreLayer.DTOs;
using CoreLayer.DTOs.Product;
using CoreLayer.Interfaces.Services;
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
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductsController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetAllProducts()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                return Ok(ApiResponse<IEnumerable<ProductDto>>.Succ(products));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<ProductDto>>.Fail(ex.Message));
            }
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ProductDto>>> GetProductById(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                    return NotFound(ApiResponse<ProductDto>.Fail($"Product with ID {id} not found"));

                return Ok(ApiResponse<ProductDto>.Succ(product));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ProductDto>.Fail(ex.Message));
            }
        }

        [AllowAnonymous]
        [HttpGet("{id}/details")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ProductDto>>> GetProductWithDetails(int id)
        {
            try
            {
                var product = await _productService.GetProductWithDetailsAsync(id);
                if (product == null)
                    return NotFound(ApiResponse<ProductDto>.Fail($"Product with ID {id} not found"));

                return Ok(ApiResponse<ProductDto>.Succ(product));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ProductDto>.Fail(ex.Message));
            }
        }

        [AllowAnonymous]
        [HttpGet("category/{categoryId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetProductsByCategory(int categoryId)
        {
            try
            {
                var categoryExists = await _categoryService.CategoryExistsAsync(categoryId);
                if (!categoryExists)
                    return NotFound(ApiResponse<IEnumerable<ProductDto>>.Fail($"Category with ID {categoryId} not found"));

                var filter = new ProductFilterDto { CategoryId = categoryId, PageSize = int.MaxValue };
                var result = await _productService.SearchProductsAsync(filter);
                return Ok(ApiResponse<IEnumerable<ProductDto>>.Succ(result.Items));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<ProductDto>>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("low-stock/{threshold}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetLowStockProducts(int threshold)
        {
            try
            {
                var products = await _productService.GetLowStockProductsAsync(threshold);
                return Ok(ApiResponse<IEnumerable<ProductDto>>.Succ(products));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<ProductDto>>.Fail(ex.Message));
            }
        }

        [AllowAnonymous]
        [HttpGet("statistics/average-price")]
        [ProducesResponseType(typeof(ApiResponse<decimal>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<decimal>>> GetAveragePrice()
        {
            try
            {
                var average = await _productService.GetAverageProductPriceAsync();
                return Ok(ApiResponse<decimal>.Succ(average));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<decimal>.Fail(ex.Message));
            }
        }

        [AllowAnonymous]
        [HttpPost("search")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PagedResult<ProductDto>>>> SearchProducts([FromBody] ProductFilterDto filter)
        {
            try
            {
                if (filter.PageNumber < 1)
                    return BadRequest(ApiResponse<PagedResult<ProductDto>>.Fail("Page number must be greater than 0"));

                if (filter.PageSize < 1 || filter.PageSize > 100)
                    return BadRequest(ApiResponse<PagedResult<ProductDto>>.Fail("Page size must be between 1 and 100"));

                var result = await _productService.SearchProductsAsync(filter);
                return Ok(ApiResponse<PagedResult<ProductDto>>.Succ(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PagedResult<ProductDto>>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct([FromBody] CreateProductDto createDto)
        {
            try
            {
                var product = await _productService.CreateProductAsync(createDto);
                return CreatedAtAction(nameof(GetProductById), new { id = product.Id },
                    ApiResponse<ProductDto>.Succ(product, "Product created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("Category"))
                    return BadRequest(ApiResponse<ProductDto>.Fail(ex.Message));
                return Conflict(ApiResponse<ProductDto>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ProductDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProduct(int id, [FromBody] UpdateProductDto updateDto)
        {
            try
            {
                var product = await _productService.UpdateProductAsync(id, updateDto);
                return Ok(ApiResponse<ProductDto>.Succ(product, "Product updated successfully"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<ProductDto>.Fail($"Product with ID {id} not found"));
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("Category"))
                    return BadRequest(ApiResponse<ProductDto>.Fail(ex.Message));
                return Conflict(ApiResponse<ProductDto>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ProductDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteProduct(int id)
        {
            try
            {
                var deleted = await _productService.DeleteProductAsync(id);
                if (!deleted)
                    return NotFound(ApiResponse<bool>.Fail($"Product with ID {id} not found"));

                return Ok(ApiResponse<bool>.Succ(true, "Product deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/stock")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateStock(int id, [FromBody] UpdateStockDto updateStockDto)
        {
            try
            {
                var updated = await _productService.UpdateStockAsync(id, updateStockDto.Quantity);
                if (!updated)
                    return NotFound(ApiResponse<bool>.Fail($"Product with ID {id} not found"));

                return Ok(ApiResponse<bool>.Succ(true, "Stock updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }
    }
}
