using System.ComponentModel.DataAnnotations;

namespace BusinessLayer.DTOs.Authentication
{
    public class LogoutRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string RefreshToken { get; set; } = null!;
    }
}
