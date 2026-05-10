namespace CoreLayer.DTOs.User
{
    public class CreateUserDto
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public DateOnly DateofBirth { get; set; }
        public string Password { get; set; } = null!;
        public string Role { get; set; } = "Customer";
    }
}
