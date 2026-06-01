using BusinessLayer.DTOs.ProductImage;
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
    public class ProductImagesController : ControllerBase
    {
        private readonly IProductImageService _productImageService;
        private readonly IProductService _productService;

        public ProductImagesController(
            IProductImageService productImageService,
            IProductService productService)
        {
            _productImageService = productImageService;
            _productService = productService;
        }

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductImageDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductImageDto>>>> GetAllImages()
        {
            try
            {
                var images = await _productImageService.GetAllAsync();
                return Ok(ApiResponse<IEnumerable<ProductImageDto>>.Succ(images));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<ProductImageDto>>.Fail(ex.Message));
            }
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProductImageDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ProductImageDto>>> GetImageById(int id)
        {
            try
            {
                var image = await _productImageService.GetByIdAsync(id);
                if (image == null)
                    return NotFound(ApiResponse<ProductImageDto>.Fail($"Image with ID {id} not found"));

                return Ok(ApiResponse<ProductImageDto>.Succ(image));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ProductImageDto>.Fail(ex.Message));
            }
        }

        [AllowAnonymous]
        [HttpGet("product/{productId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductImageDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductImageDto>>>> GetImagesByProduct(int productId)
        {
            try
            {
                var productExists = await _productService.GetByIdAsync(productId);
                if (productExists == null)
                    return NotFound(ApiResponse<IEnumerable<ProductImageDto>>.Fail($"Product with ID {productId} not found"));

                var images = await _productImageService.GetImagesByProductAsync(productId);
                return Ok(ApiResponse<IEnumerable<ProductImageDto>>.Succ(images));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<ProductImageDto>>.Fail(ex.Message));
            }
        }

        [AllowAnonymous]
        [HttpGet("product/{productId}/primary")]
        [ProducesResponseType(typeof(ApiResponse<ProductImageDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ProductImageDto>>> GetPrimaryImage(int productId)
        {
            try
            {
                var productExists = await _productService.GetByIdAsync(productId);
                if (productExists == null)
                    return NotFound(ApiResponse<ProductImageDto>.Fail($"Product with ID {productId} not found"));

                var image = await _productImageService.GetPrimaryImageAsync(productId);
                if (image == null)
                    return NotFound(ApiResponse<ProductImageDto>.Fail($"No images found for product {productId}"));

                return Ok(ApiResponse<ProductImageDto>.Succ(image));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ProductImageDto>.Fail(ex.Message));
            }
        }

        [AllowAnonymous]
        [HttpGet("product/{productId}/count")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<int>>> GetImageCount(int productId)
        {
            try
            {
                var productExists = await _productService.GetByIdAsync(productId);
                if (productExists == null)
                    return NotFound(ApiResponse<int>.Fail($"Product with ID {productId} not found"));

                var count = await _productImageService.GetImageCountByProductAsync(productId);
                return Ok(ApiResponse<int>.Succ(count));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<int>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ProductImageDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<ProductImageDto>>> CreateImage([FromBody] CreateProductImageDto createDto)
        {
            try
            {
                var image = await _productImageService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetImageById), new { id = image.Id },
                    ApiResponse<ProductImageDto>.Succ(image, "Image created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("Product"))
                    return NotFound(ApiResponse<ProductImageDto>.Fail(ex.Message));
                return BadRequest(ApiResponse<ProductImageDto>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ProductImageDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("bulk")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductImageDto>>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductImageDto>>>> BulkUploadImages([FromBody] BulkUploadImagesDto bulkDto)
        {
            try
            {
                if (bulkDto.ImageUrls == null || !bulkDto.ImageUrls.Any())
                    return BadRequest(ApiResponse<IEnumerable<ProductImageDto>>.Fail("No images provided for upload"));

                var images = await _productImageService.BulkUploadImagesAsync(bulkDto);
                return CreatedAtAction(nameof(GetImagesByProduct), new { productId = bulkDto.ProductId },
                    ApiResponse<IEnumerable<ProductImageDto>>.Succ(images, $"{images.Count()} images uploaded successfully"));
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("Product"))
                    return NotFound(ApiResponse<IEnumerable<ProductImageDto>>.Fail(ex.Message));
                return BadRequest(ApiResponse<IEnumerable<ProductImageDto>>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<ProductImageDto>>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProductImageDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<ProductImageDto>>> UpdateImage(int id, [FromBody] UpdateProductImageDto updateDto)
        {
            try
            {
                var image = await _productImageService.UpdateAsync(id, updateDto);
                return Ok(ApiResponse<ProductImageDto>.Succ(image, "Image updated successfully"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<ProductImageDto>.Fail($"Image with ID {id} not found"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ProductImageDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteImage(int id)
        {
            try
            {
                var deleted = await _productImageService.DeleteAsync(id);
                if (!deleted)
                    return NotFound(ApiResponse<bool>.Fail($"Image with ID {id} not found"));

                return Ok(ApiResponse<bool>.Succ(true, "Image deleted successfully"));
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
        public async Task<ActionResult<ApiResponse<bool>>> DeleteAllProductImages(int productId)
        {
            try
            {
                var productExists = await _productService.GetByIdAsync(productId);
                if (productExists == null)
                    return NotFound(ApiResponse<bool>.Fail($"Product with ID {productId} not found"));

                var deleted = await _productImageService.DeleteAllProductImagesAsync(productId);
                return Ok(ApiResponse<bool>.Succ(true, $"All images for product {productId} deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{imageId}/set-primary/{productId}")]
        [ProducesResponseType(typeof(ApiResponse<ProductImageDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<ProductImageDto>>> SetAsPrimary(int imageId, int productId)
        {
            try
            {
                var image = await _productImageService.SetAsPrimaryAsync(imageId, productId);
                return Ok(ApiResponse<ProductImageDto>.Succ(image, "Primary image updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<ProductImageDto>.Fail(ex.Message));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<ProductImageDto>.Fail($"Image with ID {imageId} not found"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ProductImageDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("product/{productId}/reorder")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> ReorderImages(int productId, [FromBody] ReorderImagesDto reorderDto)
        {
            try
            {
                var productExists = await _productService.GetByIdAsync(productId);
                if (productExists == null)
                    return NotFound(ApiResponse<bool>.Fail($"Product with ID {productId} not found"));

                var result = await _productImageService.ReorderImagesAsync(productId, reorderDto.ImageOrders);
                return Ok(ApiResponse<bool>.Succ(result, "Images reordered successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<bool>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("verify/{imageId}/product/{productId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> VerifyImageBelongsToProduct(int imageId, int productId)
        {
            try
            {
                var belongsToProduct = await _productImageService.ImageBelongsToProductAsync(imageId, productId);
                return Ok(ApiResponse<bool>.Succ(belongsToProduct));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }
    }
}
