using DataAccessLayer;
using System.Security.Claims;

namespace BusinessLayer.Interfaces.Services
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal? ValidateToken(string token);
        string GetEmailFromToken(string token);
        int GetUserIdFromToken(string token);
        bool ValidateRefreshToken(User user, string refreshToken);
    }
}
