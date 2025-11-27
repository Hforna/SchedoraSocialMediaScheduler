using System.Security.Claims;

namespace Schedora.Domain.Interfaces;

public interface ITokenService
{
    public (string token, DateTime expiresAt) GenerateToken(long userId, string userName, List<Claim>? claims = null);
    public DateTime GenerateRefreshTokenExpiration();
    public string GenerateRefreshToken();
}