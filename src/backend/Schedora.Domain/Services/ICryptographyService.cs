namespace Schedora.Domain.Services;

public interface ICryptographyService
{
    public byte[] CryptographyPasswordAs256Hash(string word);
}

public interface ITokensCryptographyService
{
    public string HashToken(string token);
    public bool CompareTokenHash(string token, string hash);
}