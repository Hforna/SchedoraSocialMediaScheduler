namespace Schedora.Domain.Services;

public interface IPasswordCryptographyService
{
    public string HashPassword(string password);
    public bool ValidateHash(string password, string hash);
}