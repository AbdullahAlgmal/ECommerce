using BusinessLayer.DTOs.Address;
using BusinessLayer.DTOs.User;
using BusinessLayer.Interfaces.Repositories;
using BusinessLayer.Interfaces.Services;
using DataAccessLayer;

namespace BusinessLayer.Services
{
    public class UserService : BaseService<User, UserDto, CreateUserDto, UpdateUserDto>, IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAddressRepository _addressRepository;

        public UserService(IUserRepository userRepository, IAddressRepository addressRepository) : base(userRepository)
        {
            _userRepository = userRepository;
            _addressRepository = addressRepository;
        }

        public async Task<UserDto?> GetUserWithAddressesAsync(int id)
        {
            var user = await _userRepository.GetUserWithAddressesAsync(id);
            return user != null ? await MapToDto(user) : null;
        }
        public override async Task<bool> DeleteAsync(int id)
        {
            await _addressRepository.DeleteAddressesByUserAsync(id);

            return await _userRepository.DeleteAsync(id);
        }
        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            return user != null ? await MapToDto(user) : null;
        }

        protected override async Task<UserDto> MapToDto(User user)
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
        protected override async Task<IEnumerable<UserDto>> MapToDtoList(IEnumerable<User> entities)
        {
            var userDtos = new List<UserDto>();
            foreach (var user in entities)
            {
                userDtos.Add(await MapToDto(user));
            }
            return userDtos;
        }
        protected override User MapToEntity(CreateUserDto createDto)
        {
            return new User
            {
                FirstName = createDto.FirstName,
                LastName = createDto.LastName,
                Email = createDto.Email,
                Phone = createDto.Phone,
                DateofBirth = createDto.DateofBirth,
                Role = createDto.Role,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(createDto.Password)
            };
        }
        protected override void UpdateEntity(User entity, UpdateUserDto updateDto)
        {
            entity.FirstName = updateDto.FirstName;
            entity.LastName = updateDto.LastName;
            entity.Phone = updateDto.Phone;
            entity.DateofBirth = updateDto.DateofBirth;
            entity.PasswordHash = !string.IsNullOrWhiteSpace(updateDto.Password) ? BCrypt.Net.BCrypt.HashPassword(updateDto.Password) : entity.PasswordHash;
        }
        protected override async Task ValidateBeforeCreateAsync(CreateUserDto createDto)
        {
            var isEmailUnique = await _userRepository.IsEmailUniqueAsync(createDto.Email);
            if (!isEmailUnique)
                throw new InvalidOperationException($"Email '{createDto.Email}' is already registered");

            var validRoles = new[] { "Admin", "Customer", "admin", "customer" };

            if (!validRoles.Contains(createDto.Role))
                throw new InvalidOperationException($"Role must be one of: {string.Join(", ", validRoles)}");
        }
    }
}