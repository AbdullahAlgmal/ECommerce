using CoreLayer.DTOs.Payment;
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
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;

        public PaymentsController(
            IPaymentService paymentService,
            IOrderService orderService,
            IUserService userService)
        {
            _paymentService = paymentService;
            _orderService = orderService;
            _userService = userService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PaymentDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<PaymentDto>>>> GetAllPayments()
        {
            try
            {
                var payments = await _paymentService.GetAllPaymentsAsync();
                return Ok(ApiResponse<IEnumerable<PaymentDto>>.Succ(payments));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<PaymentDto>>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaymentDto>>> GetPaymentById(int id)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByIdAsync(id);
                if (payment == null)
                    return NotFound(ApiResponse<PaymentDto>.Fail($"Payment with ID {id} not found"));

                return Ok(ApiResponse<PaymentDto>.Succ(payment));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaymentDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("order/{orderId}")]
        [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaymentDto>>> GetPaymentByOrder(int orderId)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByOrderAsync(orderId);
                if (payment == null)
                    return NotFound(ApiResponse<PaymentDto>.Fail($"No payment found for order {orderId}"));

                return Ok(ApiResponse<PaymentDto>.Succ(payment));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaymentDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<PaymentDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<PaymentDto>>>> GetPaymentsByUser(int userId)
        {
            try
            {
                var userExists = await _userService.UserExistsAsync(userId);
                if (!userExists)
                    return NotFound(ApiResponse<IEnumerable<PaymentDto>>.Fail($"User with ID {userId} not found"));

                var payments = await _paymentService.GetPaymentsByUserAsync(userId);
                return Ok(ApiResponse<IEnumerable<PaymentDto>>.Succ(payments));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<PaymentDto>>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("user/{userId}/total-spending")]
        [ProducesResponseType(typeof(ApiResponse<decimal>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<decimal>>> GetTotalUserSpending(int userId)
        {
            try
            {
                var userExists = await _userService.UserExistsAsync(userId);
                if (!userExists)
                    return NotFound(ApiResponse<decimal>.Fail($"User with ID {userId} not found"));

                var total = await _paymentService.GetTotalUserSpendingAsync(userId);
                return Ok(ApiResponse<decimal>.Succ(total));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<decimal>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaymentDto>>> ProcessPayment([FromBody] CreatePaymentDto createDto)
        {
            try
            {
                var payment = await _paymentService.ProcessPaymentAsync(createDto);
                return CreatedAtAction(nameof(GetPaymentById), new { id = payment.Id },
                    ApiResponse<PaymentDto>.Succ(payment, "Payment processed successfully"));
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("already exists"))
                    return Conflict(ApiResponse<PaymentDto>.Fail(ex.Message));
                if (ex.Message.Contains("not found"))
                    return NotFound(ApiResponse<PaymentDto>.Fail(ex.Message));
                return BadRequest(ApiResponse<PaymentDto>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaymentDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("refund")]
        [ProducesResponseType(typeof(ApiResponse<PaymentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<PaymentDto>>> RefundPayment([FromBody] RefundPaymentDto refundDto)
        {
            try
            {
                var payment = await _paymentService.RefundPaymentAsync(refundDto);
                return Ok(ApiResponse<PaymentDto>.Succ(payment, "Payment refunded successfully"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<PaymentDto>.Fail($"Payment with ID {refundDto.PaymentId} not found"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<PaymentDto>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaymentDto>.Fail(ex.Message));
            }
        }
    }
}
