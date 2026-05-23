using CoreLayer.DTOs.User;

namespace CoreLayer.DTOs.Authentication
{
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public UserDto? User { get; set; }
    }

}
