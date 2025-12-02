namespace Schedora.Domain.Services;

public interface ICryptographyService
{
    public string HashPassword(string password);
    public bool ValidateHash(string password, string hash);
    public string CryptographyPasswordAs256Hash(string word);
}