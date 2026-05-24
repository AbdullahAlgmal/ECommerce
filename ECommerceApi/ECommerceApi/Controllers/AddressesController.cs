using CoreLayer.DTOs.Address;
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
    public class AddressesController : ControllerBase
    {
        private readonly IAddressService _addressService;
        private readonly IUserService _userService;

        public AddressesController(IAddressService addressService, IUserService userService)
        {
            _addressService = addressService;
            _userService = userService;
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<AddressDto>>> GetAddressById(int id, [FromServices] IAuthorizationService authorizationService)
        {
            try
            {
                var address = await _addressService.GetAddressByIdAsync(id);
                if (address == null)
                    return NotFound(ApiResponse<AddressDto>.Fail($"Address with ID {id} not found"));

                var authResult = await authorizationService.AuthorizeAsync(User, id, "AdminOrAddressOwner");
                if (!authResult.Succeeded)
                    return Forbid();

                return Ok(ApiResponse<AddressDto>.Succ(address));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AddressDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AddressDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<IEnumerable<AddressDto>>>> GetAddressesByUser(int userId, [FromServices] IAuthorizationService authorizationService)
        {
            try
            {
                var userExists = await _userService.UserExistsAsync(userId);
                if (!userExists)
                    return NotFound(ApiResponse<IEnumerable<AddressDto>>.Fail($"User with ID {userId} not found"));

                var authResult = await authorizationService.AuthorizeAsync(User, userId, "AdminOrUserOwner");
                if (!authResult.Succeeded)
                    return Forbid();

                var addresses = await _addressService.GetAddressesByUserAsync(userId);
                return Ok(ApiResponse<IEnumerable<AddressDto>>.Succ(addresses));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<AddressDto>>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<AddressDto>>> CreateAddress([FromBody] CreateAddressDto createAddressDto)
        {
            try
            {
                var address = await _addressService.CreateAddressAsync(createAddressDto);
                return CreatedAtAction(nameof(GetAddressById), new { id = address.Id },
                    ApiResponse<AddressDto>.Succ(address, "Address created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("User"))
                    return NotFound(ApiResponse<AddressDto>.Fail(ex.Message));
                return BadRequest(ApiResponse<AddressDto>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AddressDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<AddressDto>>> UpdateAddress(int id, [FromBody] UpdateAddressDto updateAddressDto, [FromServices] IAuthorizationService authorizationService)
        {
            try
            {
                var authResult = await authorizationService.AuthorizeAsync(User, id, "AdminOrAddressOwner");
                if (!authResult.Succeeded)
                    return Forbid();

                var address = await _addressService.UpdateAddressAsync(id, updateAddressDto);
                return Ok(ApiResponse<AddressDto>.Succ(address, "Address updated successfully"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<AddressDto>.Fail($"Address with ID {id} not found"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AddressDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteAddress(int id, [FromServices] IAuthorizationService authorizationService)
        {
            try
            {
                var authResult = await authorizationService.AuthorizeAsync(User, id, "AdminOrAddressOwner");
                if (!authResult.Succeeded)
                    return Forbid();

                return Ok(ApiResponse<bool>.Succ(true, "Address deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("{addressId}/verify/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<bool>>> VerifyAddressOwnership(int addressId, int userId, [FromServices] IAuthorizationService authorizationService)
        {
            try
            {
                var authResult = await authorizationService.AuthorizeAsync(User, userId, "AdminOrUserOwner");
                if (!authResult.Succeeded)
                    return Forbid();

                var belongsToUser = await _addressService.AddressBelongsToUserAsync(addressId, userId);
                return Ok(ApiResponse<bool>.Succ(belongsToUser));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }
    }
}
