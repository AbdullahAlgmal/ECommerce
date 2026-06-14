using BusinessLayer.DTOs;
using BusinessLayer.DTOs.Order;
using BusinessLayer.DTOs.OrderItem;
using BusinessLayer.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace ECommerceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize]
    [EnableRateLimiting("ECommerceLimiter")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
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

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
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

        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrderById(int id, [FromServices] IAuthorizationService authorizationService)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                    return NotFound(ApiResponse<OrderDto>.Fail($"Order with ID {id} not found"));

                var authResult = await authorizationService.AuthorizeAsync(User, id, "AdminOrOrderOwner");
                if (!authResult.Succeeded)
                    return Forbid();

                return Ok(ApiResponse<OrderDto>.Succ(order));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<OrderDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("{id}/details")]
        [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrderWithDetails(int id, [FromServices] IAuthorizationService authorizationService)
        {
            try
            {
                var order = await _orderService.GetOrderWithDetailsAsync(id);
                if (order == null)
                    return NotFound(ApiResponse<OrderDto>.Fail($"Order with ID {id} not found"));

                var authResult = await authorizationService.AuthorizeAsync(User, id, "AdminOrOrderOwner");
                if (!authResult.Succeeded)
                    return Forbid();

                return Ok(ApiResponse<OrderDto>.Succ(order));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<OrderDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderDto>>>> GetOrdersByUser(int userId, [FromServices] IAuthorizationService authorizationService)
        {
            try
            {
                var userExists = await _userService.ExistsAsync(userId);
                if (!userExists)
                    return NotFound(ApiResponse<IEnumerable<OrderDto>>.Fail($"User with ID {userId} not found"));

                var authResult = await authorizationService.AuthorizeAsync(User, userId, "AdminOrUserOwner");
                if (!authResult.Succeeded)
                    return Forbid();

                var orders = await _orderService.GetOrdersByUserAsync(userId);
                return Ok(ApiResponse<IEnumerable<OrderDto>>.Succ(orders));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<OrderDto>>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("status/{status}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
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

        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("{id}/items")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderItemDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderItemDto>>>> GetOrderItems(int id, [FromServices] IAuthorizationService authorizationService)
        {
            try
            {
                var orderExists = await _orderService.OrderExistsAsync(id);
                if (!orderExists)
                    return NotFound(ApiResponse<IEnumerable<OrderItemDto>>.Fail($"Order with ID {id} not found"));

                var authResult = await authorizationService.AuthorizeAsync(User, id, "AdminOrOrderOwner");
                if (!authResult.Succeeded)
                    return Forbid();

                var items = await _orderItemService.GetOrderItemsByOrderAsync(id);
                return Ok(ApiResponse<IEnumerable<OrderItemDto>>.Succ(items));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<OrderItemDto>>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(ApiResponse<OrderStatisticsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
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

        [Authorize(Roles = "Admin")]
        [HttpGet("revenue")]
        [ProducesResponseType(typeof(ApiResponse<decimal>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
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

        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("can-cancel/{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> CanCancelOrder(int id, [FromServices] IAuthorizationService authorizationService)
        {
            try
            {
                var authResult = await authorizationService.AuthorizeAsync(User, id, "AdminOrOrderOwner");
                if (!authResult.Succeeded)
                    return Forbid();

                var canCancel = await _orderService.CanCancelOrderAsync(id);
                return Ok(ApiResponse<bool>.Succ(canCancel));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpPost("search")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<OrderDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<PagedResult<OrderDto>>>> SearchOrders([FromBody] OrderFilterDto filter, [FromServices] IAuthorizationService authorizationService)
        {
            try
            {
                var authResult = await authorizationService.AuthorizeAsync(User, filter.UserId, "AdminOrUserOwner");
                if (!authResult.Succeeded)
                    return Forbid();

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

        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
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

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/status")]
        [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
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

        [Authorize(Roles = "Admin,Customer")]
        [HttpPatch("{id}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<OrderDto>>> CancelOrder(int id, [FromServices] IAuthorizationService authorizationService)
        {
            try
            {
                var canCancel = await _orderService.CanCancelOrderAsync(id);
                if (!canCancel)
                    return BadRequest(ApiResponse<OrderDto>.Fail("Order cannot be cancelled in its current state"));

                var authResult = await authorizationService.AuthorizeAsync(User, id, "AdminOrOrderOwner");
                if (!authResult.Succeeded)
                    return Forbid();
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

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
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
