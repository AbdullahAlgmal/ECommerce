using BusinessLayer.DTOs.Address;
using BusinessLayer.DTOs.User;
using BusinessLayer.Interfaces.Repositories;
using BusinessLayer.Interfaces.Services;
using DataAccessLayer;

namespace BusinessLayer.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAddressRepository _addressRepository;

        public UserService(IUserRepository userRepository, IAddressRepository addressRepository)
        {
            _userRepository = userRepository;
            _addressRepository = addressRepository;
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user != null ? await MapToUserDto(user) : null;
        }
        public async Task<UserDto?> GetUserWithAddressesAsync(int id)
        {
            var user = await _userRepository.GetUserWithAddressesAsync(id);
            return user != null ? await MapToUserDto(user) : null;
        }
        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            var isEmailUnique = await _userRepository.IsEmailUniqueAsync(createUserDto.Email);
            if (!isEmailUnique)
                throw new InvalidOperationException($"Email '{createUserDto.Email}' is already registered");

            var validRoles = new[] { "Admin", "Customer", "Seller" };
            if (!validRoles.Contains(createUserDto.Role))
                throw new InvalidOperationException($"Role must be one of: {string.Join(", ", validRoles)}");

            var user = new User
            {
                FirstName = createUserDto.FirstName,
                LastName = createUserDto.LastName,
                Email = createUserDto.Email,
                Phone = createUserDto.Phone,
                DateofBirth = createUserDto.DateofBirth,
                Role = createUserDto.Role,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password)
            };

            var createdUser = await _userRepository.AddAsync(user);
            return await MapToUserDto(createdUser);
        }
        public async Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} not found");

            user.FirstName = updateUserDto.FirstName;
            user.LastName = updateUserDto.LastName;
            user.Phone = updateUserDto.Phone;
            user.DateofBirth = updateUserDto.DateofBirth;

            if (!string.IsNullOrWhiteSpace(updateUserDto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateUserDto.Password);
            }

            var updatedUser = await _userRepository.UpdateAsync(user);
            return await MapToUserDto(updatedUser);
        }
        public async Task<bool> DeleteUserAsync(int id)
        {
            await _addressRepository.DeleteAddressesByUserAsync(id);

            return await _userRepository.DeleteAsync(id);
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            return user != null ? await MapToUserDto(user) : null;
        }
        public async Task<bool> UserExistsAsync(int id)
        {
            return await _userRepository.ExistsAsync(u => u.Id == id);
        }
        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _userRepository.CountAsync();
        }

        private async Task<UserDto> MapToUserDto(User user)
        {
            var addresses = await _addressRepository.GetAddressesByUserAsync(user.Id);

            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                DateofBirth = user.DateofBirth,
                Role = user.Role,
                Addresses = addresses.Select(a => new AddressDto
                {
                    Id = a.Id,
                    HouseNumber = a.HouseNumber,
                    StreetBlock = a.StreetBlock,
                    Area = a.Area,
                    City = a.City,
                    Province = a.Province,
                    Country = a.Country,
                    ZipCode = a.ZipCode,
                    UserId = a.UserId,
                    UserName = $"{user.FirstName} {user.LastName}"
                }).ToList()
            };
        }
    }
}
