using BusinessLayer.DTOs.Authentication;
using BusinessLayer.DTOs.User;
using BusinessLayer.Interfaces.Repositories;
using BusinessLayer.Interfaces.Services;
using DataAccessLayer;

namespace BusinessLayer.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;

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

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshTokenHash = HashRefreshToken(refreshToken);
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
            user.RefreshTokenRevokedAt = null;
            await _userRepository.UpdateAsync(user);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login successful",
                Token = accessToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = user.RefreshTokenExpiresAt.Value,
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

            var today = DateOnly.FromDateTime(DateTime.Today);
            var age = today.Year - registerDto.DateofBirth.Year;
            if (DateOnly.FromDateTime(registerDto.DateofBirth) > today.AddYears(-age)) age--;

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

            var validRoles = new[] { "Admin", "Customer", "Seller", "Manager" };
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
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            };

            var createdUser = await _userRepository.AddAsync(user);
            var accessToken = _jwtService.GenerateAccessToken(createdUser);
            var refreshToken = _jwtService.GenerateRefreshToken();

            createdUser.RefreshTokenHash = HashRefreshToken(refreshToken);
            createdUser.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateAsync(createdUser);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Registration successful",
                Token = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(30),
                RefreshTokenExpiresAt = createdUser.RefreshTokenExpiresAt.Value,
                User = MapToUserDto(createdUser)
            };
        }
        public async Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            if (!_jwtService.ValidateRefreshToken(user, request.RefreshToken))
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token");
            }

            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshTokenHash = HashRefreshToken(newRefreshToken);
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
            user.RefreshTokenRevokedAt = null;
            await _userRepository.UpdateAsync(user);

            return new TokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(30),
                RefreshTokenExpiresAt = user.RefreshTokenExpiresAt.Value
            };
        }
        public async Task<bool> LogoutAsync(LogoutRequestDto request)
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email);

            if (user == null)
            {
                return false;
            }

            if (!_jwtService.ValidateRefreshToken(user, request.RefreshToken))
            {
                return false;
            }

            user.RefreshTokenRevokedAt = DateTime.UtcNow;
            user.RefreshTokenHash = null;
            await _userRepository.UpdateAsync(user);

            return true;
        }
        public async Task<bool> RevokeAllUserTokensAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return false;
            }

            user.RefreshTokenRevokedAt = DateTime.UtcNow;
            user.RefreshTokenHash = null;
            user.RefreshTokenExpiresAt = null;
            await _userRepository.UpdateAsync(user);

            return true;
        }
        public async Task<bool> ValidateTokenAsync(string token)
        {
            var principal = _jwtService.ValidateToken(token);
            if (principal == null)
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

        private string HashRefreshToken(string refreshToken)
        {
            return BCrypt.Net.BCrypt.HashPassword(refreshToken);
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
                Role = user.Role,
            };
        }
    }
}
