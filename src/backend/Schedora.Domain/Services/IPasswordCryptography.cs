namespace Schedora.Domain.Services;

public interface IPasswordCryptography
{
    public string HashPassword(string password);
    public bool ValidateHash(string password, string hash);
}