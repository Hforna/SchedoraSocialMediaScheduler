using Schedora.Domain.Services;

namespace Schedora.Infrastructure.Services;

public class TokensHashService : ITokensCryptographyService
{
    public string HashToken(string token) => BCrypt.Net.BCrypt.HashPassword(token);

    public bool CompareTokenHash(string token, string hash) => BCrypt.Net.BCrypt.Verify(token, hash);
}