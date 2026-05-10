namespace CoreLayer.DTOs.User
{
    public class UpdateUserDto
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public DateOnly DateofBirth { get; set; }
        public string? Password { get; set; }
    }
}
