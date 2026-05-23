using ECommerceApi;
using System.Security.Claims;

namespace CoreLayer.Interfaces.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        ClaimsPrincipal? ValidateToken(string token);
        string GetEmailFromToken(string token);
        int GetUserIdFromToken(string token);
    }
}
