using System.Security.Claims;

namespace UavFleetManagement.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateToken(int id, string username, string role, int expiryMinutes = 30);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
