using CoreLayer.DTOs.Address;

namespace CoreLayer.DTOs.User
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public DateOnly DateofBirth { get; set; }
        public string Role { get; set; } = null!;
        public List<AddressDto> Addresses { get; set; } = new();
    }
}
