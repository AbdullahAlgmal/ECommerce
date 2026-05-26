using BusinessLayer.DTOs.User;

namespace BusinessLayer.DTOs.Authentication
{
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public string Token { get; set; } = null!;
        public string? RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? AccessTokenExpiresAt { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }
        public UserDto? User { get; set; }
    }

}
