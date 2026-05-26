using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.DTOs.Authentication
{
    public class RefreshTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}
