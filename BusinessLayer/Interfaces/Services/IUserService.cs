using BusinessLayer.DTOs.User;
using DataAccessLayer;

namespace BusinessLayer.Interfaces.Services
{
    public interface IUserService : IBaseService<User, UserDto, CreateUserDto, UpdateUserDto>
    {
        Task<UserDto?> GetUserWithAddressesAsync(int id);
        Task<UserDto?> GetUserByEmailAsync(string email);
    }
}
