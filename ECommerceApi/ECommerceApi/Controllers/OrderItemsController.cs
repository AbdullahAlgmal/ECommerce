using CoreLayer.DTOs.OrderItem;
using CoreLayer.DTOs.Product;
using CoreLayer.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize]
    public class OrderItemsController : ControllerBase
    {
        private readonly IOrderItemService _orderItemService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;

        public OrderItemsController(
            IOrderItemService orderItemService,
            IOrderService orderService,
            IProductService productService)
        {
            _orderItemService = orderItemService;
            _orderService = orderService;
            _productService = productService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderItemDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderItemDto>>>> GetAllOrderItems()
        {
            try
            {
                var items = await _orderItemService.GetAllOrderItemsAsync();
                return Ok(ApiResponse<IEnumerable<OrderItemDto>>.Succ(items));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<OrderItemDto>>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<OrderItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<OrderItemDto>>> GetOrderItemById(int id)
        {
            try
            {
                var item = await _orderItemService.GetOrderItemByIdAsync(id);
                if (item == null)
                    return NotFound(ApiResponse<OrderItemDto>.Fail($"Order item with ID {id} not found"));

                return Ok(ApiResponse<OrderItemDto>.Succ(item));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<OrderItemDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("order/{orderId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderItemDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderItemDto>>>> GetOrderItemsByOrder(int orderId)
        {
            try
            {
                var orderExists = await _orderService.OrderExistsAsync(orderId);
                if (!orderExists)
                    return NotFound(ApiResponse<IEnumerable<OrderItemDto>>.Fail($"Order with ID {orderId} not found"));

                var items = await _orderItemService.GetOrderItemsByOrderAsync(orderId);
                return Ok(ApiResponse<IEnumerable<OrderItemDto>>.Succ(items));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<OrderItemDto>>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("product/{productId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderItemDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderItemDto>>>> GetOrderItemsByProduct(int productId)
        {
            try
            {
                var productExists = await _productService.GetProductByIdAsync(productId);
                if (productExists == null)
                    return NotFound(ApiResponse<IEnumerable<OrderItemDto>>.Fail($"Product with ID {productId} not found"));

                var items = await _orderItemService.GetOrderItemsByProductAsync(productId);
                return Ok(ApiResponse<IEnumerable<OrderItemDto>>.Succ(items));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<OrderItemDto>>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(ApiResponse<OrderItemStatisticsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<OrderItemStatisticsDto>>> GetStatistics()
        {
            try
            {
                var statistics = await _orderItemService.GetOrderItemStatisticsAsync();
                return Ok(ApiResponse<OrderItemStatisticsDto>.Succ(statistics));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<OrderItemStatisticsDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("top-selling/{count}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductSalesDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductSalesDto>>>> GetTopSellingProducts(int count)
        {
            try
            {
                if (count < 1 || count > 100)
                    return BadRequest(ApiResponse<IEnumerable<ProductSalesDto>>.Fail("Count must be between 1 and 100"));

                var topProducts = await _orderItemService.GetTopSellingProductsAsync(count);
                return Ok(ApiResponse<IEnumerable<ProductSalesDto>>.Succ(topProducts));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<ProductSalesDto>>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("product/{productId}/total-sales")]
        [ProducesResponseType(typeof(ApiResponse<decimal>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<decimal>>> GetTotalSalesByProduct(int productId)
        {
            try
            {
                var productExists = await _productService.GetProductByIdAsync(productId);
                if (productExists == null)
                    return NotFound(ApiResponse<decimal>.Fail($"Product with ID {productId} not found"));

                var totalSales = await _orderItemService.GetTotalSalesByProductAsync(productId);
                return Ok(ApiResponse<decimal>.Succ(totalSales));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<decimal>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<OrderItemDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<OrderItemDto>>> CreateOrderItem([FromBody] CreateOrderItemDto createDto)
        {
            try
            {
                var item = await _orderItemService.CreateOrderItemAsync(createDto);
                return CreatedAtAction(nameof(GetOrderItemById), new { id = item.Id },
                    ApiResponse<OrderItemDto>.Succ(item, "Order item created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("Order"))
                    return NotFound(ApiResponse<OrderItemDto>.Fail(ex.Message));
                if (ex.Message.Contains("stock"))
                    return Conflict(ApiResponse<OrderItemDto>.Fail(ex.Message));
                return BadRequest(ApiResponse<OrderItemDto>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<OrderItemDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<OrderItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<OrderItemDto>>> UpdateOrderItem(int id, [FromBody] UpdateOrderItemDto updateDto)
        {
            try
            {
                var item = await _orderItemService.UpdateOrderItemAsync(id, updateDto);
                return Ok(ApiResponse<OrderItemDto>.Succ(item, "Order item updated successfully"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<OrderItemDto>.Fail($"Order item with ID {id} not found"));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<OrderItemDto>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<OrderItemDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteOrderItem(int id)
        {
            try
            {
                var deleted = await _orderItemService.DeleteOrderItemAsync(id);
                if (!deleted)
                    return NotFound(ApiResponse<bool>.Fail($"Order item with ID {id} not found"));

                return Ok(ApiResponse<bool>.Succ(true, "Order item deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("order/{orderId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteOrderItemsByOrder(int orderId)
        {
            try
            {
                var orderExists = await _orderService.OrderExistsAsync(orderId);
                if (!orderExists)
                    return NotFound(ApiResponse<bool>.Fail($"Order with ID {orderId} not found"));

                var deleted = await _orderItemService.DeleteOrderItemsByOrderAsync(orderId);
                return Ok(ApiResponse<bool>.Succ(deleted, $"All items for order {orderId} deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }
    }
}
