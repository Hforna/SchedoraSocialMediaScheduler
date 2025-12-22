namespace Schedora.Domain.Services;

public interface ICryptographyService
{
    public byte[] CryptographyPasswordAs256Hash(string word);
}

public interface ITokensCryptographyService
{
    public string EncryptToken(string token);
    public string DecryptToken(string token);
}