using CoreLayer.DTOs.Authentication;
using CoreLayer.DTOs.User;

namespace CoreLayer.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<bool> LogoutAsync(int userId);
        Task<bool> ValidateTokenAsync(string token);
        Task<UserDto?> GetCurrentUserAsync(string token);
    }
}
