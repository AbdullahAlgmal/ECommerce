using BusinessLayer.DTOs.User;

namespace BusinessLayer.Interfaces.Services
{
    public interface IUserService
    {
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto?> GetUserWithAddressesAsync(int id);
        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
        Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(int id);

        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<bool> UserExistsAsync(int id);

        Task<int> GetTotalUsersCountAsync();
    }
}
