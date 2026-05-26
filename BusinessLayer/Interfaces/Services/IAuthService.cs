using BusinessLayer.DTOs.Authentication;
using BusinessLayer.DTOs.User;

namespace BusinessLayer.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
        Task<bool> LogoutAsync(LogoutRequestDto request);
        Task<bool> RevokeAllUserTokensAsync(int userId);
        Task<bool> ValidateTokenAsync(string token);
        Task<UserDto?> GetCurrentUserAsync(string token);
    }
}
