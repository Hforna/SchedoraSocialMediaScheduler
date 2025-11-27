using System.Security.Claims;

namespace Schedora.Domain.Services;

public interface ITokenService
{
    public (string token, DateTime expiresAt) GenerateToken(long userId, string userName, List<Claim>? claims = null);
    public DateTime GenerateRefreshTokenExpiration();
    public string GenerateRefreshToken();
    public Task<User?> GetUserByToken();
    public List<Claim>? GetTokenClaims();
}