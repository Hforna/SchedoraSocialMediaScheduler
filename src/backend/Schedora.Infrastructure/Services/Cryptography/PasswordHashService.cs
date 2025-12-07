using Schedora.Domain.Services;

namespace Schedora.Infrastructure.Services;

public class PasswordHashService : IPasswordCryptographyService
{
    public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool ValidateHash(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}