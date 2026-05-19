using CoreLayer.DTOs.Address;
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
    public class AddressesController : ControllerBase
    {
        private readonly IAddressService _addressService;
        private readonly IUserService _userService;

        public AddressesController(IAddressService addressService, IUserService userService)
        {
            _addressService = addressService;
            _userService = userService;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<AddressDto>>> GetAddressById(int id)
        {
            try
            {
                var address = await _addressService.GetAddressByIdAsync(id);
                if (address == null)
                    return NotFound(ApiResponse<AddressDto>.Fail($"Address with ID {id} not found"));

                return Ok(ApiResponse<AddressDto>.Succ(address));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AddressDto>.Fail(ex.Message));
            }
        }

        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AddressDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<AddressDto>>>> GetAddressesByUser(int userId)
        {
            try
            {
                var userExists = await _userService.UserExistsAsync(userId);
                if (!userExists)
                    return NotFound(ApiResponse<IEnumerable<AddressDto>>.Fail($"User with ID {userId} not found"));

                var addresses = await _addressService.GetAddressesByUserAsync(userId);
                return Ok(ApiResponse<IEnumerable<AddressDto>>.Succ(addresses));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<AddressDto>>.Fail(ex.Message));
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
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

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<AddressDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<AddressDto>>> UpdateAddress(int id, [FromBody] UpdateAddressDto updateAddressDto)
        {
            try
            {
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

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteAddress(int id)
        {
            try
            {
                var deleted = await _addressService.DeleteAddressAsync(id);
                if (!deleted)
                    return NotFound(ApiResponse<bool>.Fail($"Address with ID {id} not found"));

                return Ok(ApiResponse<bool>.Succ(true, "Address deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }

        [HttpGet("{addressId}/verify/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> VerifyAddressOwnership(int addressId, int userId)
        {
            try
            {
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
