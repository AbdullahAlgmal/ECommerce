using CoreLayer.DTOs.Shipping;
using CoreLayer.Interfaces.Repositories;
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
    public class ShippingsController : ControllerBase
    {
        private readonly IShippingService _shippingService;
        private readonly IOrderService _orderService;
        private readonly IAddressRepository _addressRepository;

        public ShippingsController(
            IShippingService shippingService,
            IOrderService orderService,
            IAddressRepository addressRepository)
        {
            _shippingService = shippingService;
            _orderService = orderService;
            _addressRepository = addressRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<ShippingDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<ShippingDto>>>> GetAllShippings()
        {
            try
            {
                var shippings = await _shippingService.GetAllShippingsAsync();
                return Ok(ApiResponse<IEnumerable<ShippingDto>>.Succ(shippings));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<ShippingDto>>.Fail(ex.Message));
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ShippingDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ShippingDto>>> GetShippingById(int id)
        {
            try
            {
                var shipping = await _shippingService.GetShippingByIdAsync(id);
                if (shipping == null)
                    return NotFound(ApiResponse<ShippingDto>.Fail($"Shipping with ID {id} not found"));

                return Ok(ApiResponse<ShippingDto>.Succ(shipping));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ShippingDto>.Fail(ex.Message));
            }
        }

        [HttpGet("order/{orderId}")]
        [ProducesResponseType(typeof(ApiResponse<ShippingDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ShippingDto>>> GetShippingByOrder(int orderId)
        {
            try
            {
                var shipping = await _shippingService.GetShippingByOrderAsync(orderId);
                if (shipping == null)
                    return NotFound(ApiResponse<ShippingDto>.Fail($"No shipping found for order {orderId}"));

                return Ok(ApiResponse<ShippingDto>.Succ(shipping));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ShippingDto>.Fail(ex.Message));
            }
        }

        [HttpGet("track/{trackingNumber}")]
        [ProducesResponseType(typeof(ApiResponse<ShippingTrackingResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ShippingTrackingResult>>> TrackShipment(string trackingNumber)
        {
            try
            {
                var tracking = await _shippingService.TrackShipmentAsync(trackingNumber);
                if (!tracking.IsSuccess)
                    return NotFound(ApiResponse<ShippingTrackingResult>.Fail($"Tracking number {trackingNumber} not found"));

                return Ok(ApiResponse<ShippingTrackingResult>.Succ(tracking));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ShippingTrackingResult>.Fail(ex.Message));
            }
        }

        [HttpPost("calculate-rate")]
        [ProducesResponseType(typeof(ApiResponse<ShippingRateResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ShippingRateResult>>> CalculateRate(
            [FromQuery] int addressId,
            [FromQuery] decimal totalWeight,
            [FromQuery] int totalItems)
        {
            try
            {
                var rate = await _shippingService.CalculateShippingRateAsync(addressId, totalWeight, totalItems);
                return Ok(ApiResponse<ShippingRateResult>.Succ(rate));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<ShippingRateResult>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ShippingRateResult>.Fail(ex.Message));
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ShippingDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ShippingDto>>> CreateShipping([FromBody] CreateShippingDto createDto)
        {
            try
            {
                var shipping = await _shippingService.CreateShippingAsync(createDto);
                return CreatedAtAction(nameof(GetShippingById), new { id = shipping.Id },
                    ApiResponse<ShippingDto>.Succ(shipping, "Shipping created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("already exists"))
                    return Conflict(ApiResponse<ShippingDto>.Fail(ex.Message));
                if (ex.Message.Contains("not found"))
                    return NotFound(ApiResponse<ShippingDto>.Fail(ex.Message));
                return BadRequest(ApiResponse<ShippingDto>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ShippingDto>.Fail(ex.Message));
            }
        }

        [HttpPatch("{id}/status")]
        [ProducesResponseType(typeof(ApiResponse<ShippingDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ShippingDto>>> UpdateShippingStatus(int id, [FromBody] UpdateShippingStatusDto updateDto)
        {
            try
            {
                var shipping = await _shippingService.UpdateShippingStatusAsync(id, updateDto);
                return Ok(ApiResponse<ShippingDto>.Succ(shipping, "Shipping status updated successfully"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<ShippingDto>.Fail($"Shipping with ID {id} not found"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ShippingDto>.Fail(ex.Message));
            }
        }

        [HttpPost("{id}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> CancelShipping(int id)
        {
            try
            {
                var cancelled = await _shippingService.CancelShippingAsync(id);
                if (!cancelled)
                    return NotFound(ApiResponse<bool>.Fail($"Shipping with ID {id} not found or cannot be cancelled"));

                return Ok(ApiResponse<bool>.Succ(true, "Shipping cancelled successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }
    }
}
