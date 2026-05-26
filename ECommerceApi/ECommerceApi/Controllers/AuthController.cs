using BusinessLayer.DTOs.Authentication;
using BusinessLayer.DTOs.User;
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
    [EnableRateLimiting("ECommerceLimiter")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var result = await _authService.LoginAsync(loginDto);

                if (!result.Success)
                {
                    return Unauthorized(ApiResponse<AuthResponseDto>.Fail(result.Message));
                }

                return Ok(ApiResponse<AuthResponseDto>.Succ(result, result.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AuthResponseDto>.Fail(ex.Message));
            }
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                var result = await _authService.RegisterAsync(registerDto);

                if (!result.Success)
                {
                    if (result.Message.Contains("already registered"))
                        return Conflict(ApiResponse<AuthResponseDto>.Fail(result.Message));

                    return BadRequest(ApiResponse<AuthResponseDto>.Fail(result.Message));
                }

                return Ok(ApiResponse<AuthResponseDto>.Succ(result, result.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<AuthResponseDto>.Fail(ex.Message));
            }
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<TokenResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<TokenResponseDto>>> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(request);
                return Ok(ApiResponse<TokenResponseDto>.Succ(result, "Token refreshed successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<TokenResponseDto>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<TokenResponseDto>.Fail(ex.Message));
            }
        }

        [HttpPost("logout")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<bool>>> Logout([FromBody] LogoutRequestDto request)
        {
            try
            {
                var result = await _authService.LogoutAsync(request);
                if (!result)
                    return BadRequest(ApiResponse<bool>.Fail("Logout failed"));

                return Ok(ApiResponse<bool>.Succ(true, "Logged out successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }

        [Authorize]
        [HttpPost("revoke-all-tokens")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<bool>>> RevokeAllTokens()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _authService.RevokeAllUserTokensAsync(userId);

                if (!result)
                    return BadRequest(ApiResponse<bool>.Fail("Failed to revoke tokens"));

                return Ok(ApiResponse<bool>.Succ(true, "All tokens revoked successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }

        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
        {
            try
            {
                var token = GetTokenFromRequest();
                var user = await _authService.GetCurrentUserAsync(token);

                if (user == null)
                    return NotFound(ApiResponse<UserDto>.Fail("User not found"));

                return Ok(ApiResponse<UserDto>.Succ(user));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserDto>.Fail(ex.Message));
            }
        }

        [Authorize]
        [HttpGet("validate")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<bool>>> ValidateToken()
        {
            try
            {
                var token = GetTokenFromRequest();
                var isValid = await _authService.ValidateTokenAsync(token);

                if (!isValid)
                    return Unauthorized(ApiResponse<bool>.Fail("Invalid token"));

                return Ok(ApiResponse<bool>.Succ(true, "Token is valid"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Fail(ex.Message));
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value ??
                             User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
        private string GetTokenFromRequest()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return string.Empty;

            return authHeader.Substring("Bearer ".Length).Trim();
        }
    }
}
