using System.Security.Claims;

namespace Schedora.Domain.Interfaces;

public interface ITokenService
{
    public string GenerateToken(long userId, string userName, List<Claim>? claims = null);
}