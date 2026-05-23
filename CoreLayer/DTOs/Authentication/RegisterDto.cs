using System.ComponentModel.DataAnnotations;

namespace CoreLayer.DTOs.Authentication
{
    public class RegisterDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string LastName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [Phone]
        public string Phone { get; set; } = null!;

        [Required]
        public DateOnly DateofBirth { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = null!;

        public string Role { get; set; } = "Customer";
    }
}
