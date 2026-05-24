using CoreLayer.DTOs.Authentication;
using CoreLayer.DTOs.User;
using CoreLayer.Interfaces.Repositories;
using CoreLayer.Interfaces.Services;
using ECommerceApi;

namespace BusinessLayer.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private static readonly Dictionary<int, DateTime> _blacklistedTokens = new();

        public AuthService(IUserRepository userRepository, IJwtService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetUserByEmailAsync(loginDto.Email);

            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password",
                    Token = string.Empty,
                    ExpiresAt = DateTime.MinValue
                };
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password",
                    Token = string.Empty,
                    ExpiresAt = DateTime.MinValue
                };
            }

            var token = _jwtService.GenerateToken(user);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                User = MapToUserDto(user)
            };
        }
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email already registered",
                    Token = string.Empty,
                    ExpiresAt = DateTime.MinValue
                };
            }

            var today = DateTime.Today;
            var age = today.Year - registerDto.DateofBirth.Year;
            if (registerDto.DateofBirth > today.AddYears(-age)) age--;

            if (age < 18)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "You must be at least 18 years old to register",
                    Token = string.Empty,
                    ExpiresAt = DateTime.MinValue
                };
            }

            var validRoles = new[] { "Admin", "Customer"};
            if (!validRoles.Contains(registerDto.Role))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = $"Role must be one of: {string.Join(", ", validRoles)}",
                    Token = string.Empty,
                    ExpiresAt = DateTime.MinValue
                };
            }

            var user = new User
            {
                FirstName = registerDto.FirstName.Trim(),
                LastName = registerDto.LastName.Trim(),
                Email = registerDto.Email.Trim().ToLower(),
                Phone = registerDto.Phone,
                DateofBirth = DateOnly.FromDateTime(registerDto.DateofBirth),
                Role = registerDto.Role,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password)
            };

            var createdUser = await _userRepository.AddAsync(user);
            var token = _jwtService.GenerateToken(createdUser);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Registration successful",
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                User = MapToUserDto(createdUser)
            };
        }
        public async Task<bool> LogoutAsync(int userId)
        {
            _blacklistedTokens[userId] = DateTime.UtcNow;
            await Task.CompletedTask;
            return true;
        }
        public async Task<bool> ValidateTokenAsync(string token)
        {
            var principal = _jwtService.ValidateToken(token);
            if (principal == null)
                return false;

            var userId = _jwtService.GetUserIdFromToken(token);

            if (_blacklistedTokens.ContainsKey(userId))
                return false;

            await Task.CompletedTask;
            return true;
        }
        public async Task<UserDto?> GetCurrentUserAsync(string token)
        {
            var userId = _jwtService.GetUserIdFromToken(token);
            if (userId == 0)
                return null;

            var user = await _userRepository.GetByIdAsync(userId);
            return user != null ? MapToUserDto(user) : null;
        }

        private UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                DateofBirth = user.DateofBirth,
                Role = user.Role
            };
        }
    }
}
