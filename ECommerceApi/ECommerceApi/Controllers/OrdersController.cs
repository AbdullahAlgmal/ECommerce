using CoreLayer.DTOs;
using CoreLayer.DTOs.Order;
using CoreLayer.DTOs.OrderItem;
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
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IOrderItemService _orderItemService;
        private readonly IUserService _userService;

        public OrdersController(
            IOrderService orderService,
            IOrderItemService orderItemService,
            IUserService userService)
        {
            _orderService = orderService;
            _orderItemService = orderItemService;
            _userService = userService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderDto>>>> GetAllOrders()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                return Ok(ApiResponse<IEnumerable<OrderDto>>.Succ(orders));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<OrderDto>>.Fail(ex.Message));
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrderById(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                    return NotFound(ApiResponse<OrderDto>.Fail($"Order with ID {id} not found"));

                return Ok(ApiResponse<OrderDto>.Succ(order));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<OrderDto>.Fail(ex.Message));
            }
        }

        [HttpGet("{id}/details")]
        [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrderWithDetails(int id)
        {
            try
            {
                var order = await _orderService.GetOrderWithDetailsAsync(id);
                if (order == null)
                    return NotFound(ApiResponse<OrderDto>.Fail($"Order with ID {id} not found"));

                return Ok(ApiResponse<OrderDto>.Succ(order));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<OrderDto>.Fail(ex.Message));
            }
        }

        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderDto>>>> GetOrdersByUser(int userId)
        {
            try
            {
                var userExists = await _userService.UserExistsAsync(userId);
                if (!userExists)
                    return NotFound(ApiResponse<IEnumerable<OrderDto>>.Fail($"User with ID {userId} not found"));

                var orders = await _orderService.GetOrdersByUserAsync(userId);
                return Ok(ApiResponse<IEnumerable<OrderDto>>.Succ(orders));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<OrderDto>>.Fail(ex.Message));
            }
        }

        [HttpGet("status/{status}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderDto>>>> GetOrdersByStatus(byte status)
        {
            try
            {
                var orders = await _orderService.GetOrdersByStatusAsync(status);
                return Ok(ApiResponse<IEnumerable<OrderDto>>.Succ(orders));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<OrderDto>>.Fail(ex.Message));
            }
        }

        [HttpGet("{id}/items")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderItemDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderItemDto>>>> GetOrderItems(int id)
        {
            try
            {
                var orderExists = await _orderService.OrderExistsAsync(id);
                if (!orderExists)
                    return NotFound(ApiResponse<IEnumerable<OrderItemDto>>.Fail($"Order with ID {id} not found"));

                var items = await _orderItemService.GetOrderItemsByOrderAsync(id);
                return Ok(ApiResponse<IEnumerable<OrderItemDto>>.Succ(items));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<OrderItemDto>>.Fail(ex.Message));
            }
        }

        [HttpGet("statistics")]
        [ProducesResponseType(typeof(ApiResponse<OrderStatisticsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<OrderStatisticsDto>>> GetStatistics()
        {
            try
            {
                var statistics = await _orderService.GetOrderStatisticsAsync();
                return Ok(ApiResponse<OrderStatisticsDto>.Succ(statistics));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<OrderStatisticsDto>.Fail(ex.Message));
            }
        }

        [HttpGet("revenue")]
        [ProducesResponseType(typeof(ApiResponse<decimal>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<decimal>>> GetRevenue([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
        {
            try
            {
                var revenue = await _orderService.GetTotalRevenueAsync(fromDate, toDate);
                return Ok(ApiResponse<decimal>.Succ(revenue));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<decimal>.Fail(ex.Message));
            }
        }

        [HttpGet("can-cancel/{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> CanCancelOrder(int id)
        {
            try
            {
                var canCancel = await _orderService.CanCancelOrderAsync(id);
                return Ok(ApiResponse<bool>.Succ(canCancel));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }

        [HttpPost("search")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<OrderDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PagedResult<OrderDto>>>> SearchOrders([FromBody] OrderFilterDto filter)
        {
            try
            {
                if (filter.PageNumber < 1)
                    return BadRequest(ApiResponse<PagedResult<OrderDto>>.Fail("Page number must be greater than 0"));

                if (filter.PageSize < 1 || filter.PageSize > 100)
                    return BadRequest(ApiResponse<PagedResult<OrderDto>>.Fail("Page size must be between 1 and 100"));

                var result = await _orderService.SearchOrdersAsync(filter);
                return Ok(ApiResponse<PagedResult<OrderDto>>.Succ(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PagedResult<OrderDto>>.Fail(ex.Message));
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrder([FromBody] CreateOrderDto createDto)
        {
            try
            {
                var order = await _orderService.CreateOrderAsync(createDto);
                return CreatedAtAction(nameof(GetOrderById), new { id = order.Id },
                    ApiResponse<OrderDto>.Succ(order, "Order created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("User"))
                    return NotFound(ApiResponse<OrderDto>.Fail(ex.Message));
                if (ex.Message.Contains("stock"))
                    return Conflict(ApiResponse<OrderDto>.Fail(ex.Message));
                return BadRequest(ApiResponse<OrderDto>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<OrderDto>.Fail(ex.Message));
            }
        }

        [HttpPatch("{id}/status")]
        [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<OrderDto>>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto statusDto)
        {
            try
            {
                var order = await _orderService.UpdateOrderStatusAsync(id, statusDto.Status);
                return Ok(ApiResponse<OrderDto>.Succ(order, "Order status updated successfully"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<OrderDto>.Fail($"Order with ID {id} not found"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<OrderDto>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<OrderDto>.Fail(ex.Message));
            }
        }

        [HttpPatch("{id}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<OrderDto>>> CancelOrder(int id)
        {
            try
            {
                var canCancel = await _orderService.CanCancelOrderAsync(id);
                if (!canCancel)
                    return BadRequest(ApiResponse<OrderDto>.Fail("Order cannot be cancelled in its current state"));

                var order = await _orderService.UpdateOrderStatusAsync(id, 5); // 5 = Cancelled
                return Ok(ApiResponse<OrderDto>.Succ(order, "Order cancelled successfully"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<OrderDto>.Fail($"Order with ID {id} not found"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<OrderDto>.Fail(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteOrder(int id)
        {
            try
            {
                var deleted = await _orderService.DeleteOrderAsync(id);
                if (!deleted)
                    return NotFound(ApiResponse<bool>.Fail($"Order with ID {id} not found"));

                return Ok(ApiResponse<bool>.Succ(true, "Order deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }
    }
}
