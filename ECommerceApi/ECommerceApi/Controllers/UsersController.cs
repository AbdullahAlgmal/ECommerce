using CoreLayer.DTOs;
using CoreLayer.DTOs.User;
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
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(ApiResponse<UserDto>.Fail($"User with ID {id} not found"));

                return Ok(ApiResponse<UserDto>.Succ(user));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpGet("email/{email}")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetUserByEmail(string email)
        {
            try
            {
                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                    return NotFound(ApiResponse<UserDto>.Fail($"User with email {email} not found"));

                return Ok(ApiResponse<UserDto>.Succ(user));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<int>>> GetStatistics()
        {
            try
            {
                var statistics = await _userService.GetTotalUsersCountAsync();
                return Ok(ApiResponse<int>.Succ(statistics));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<int>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                var user = await _userService.CreateUserAsync(createUserDto);
                return CreatedAtAction(nameof(GetUser), new { id = user.Id },
                    ApiResponse<UserDto>.Succ(user, "User created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("Email"))
                    return Conflict(ApiResponse<UserDto>.Fail(ex.Message));
                return BadRequest(ApiResponse<UserDto>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                var user = await _userService.UpdateUserAsync(id, updateUserDto);
                return Ok(ApiResponse<UserDto>.Succ(user, "User updated successfully"));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(ApiResponse<UserDto>.Fail($"User with ID {id} not found"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserDto>.Fail(ex.Message));
            }
        }

        [Authorize(Roles = "Admin,Customer")]
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(int id)
        {
            try
            {
                var deleted = await _userService.DeleteUserAsync(id);
                if (!deleted)
                    return NotFound(ApiResponse<bool>.Fail($"User with ID {id} not found"));

                return Ok(ApiResponse<bool>.Succ(true, "User deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }
    }
}
