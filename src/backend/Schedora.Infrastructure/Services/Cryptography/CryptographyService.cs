using System.Security.Cryptography;
using System.Text;
using Schedora.Domain.Services;

namespace Schedora.Infrastructure.Services;

public class CryptographyService : ICryptographyService
{
    public byte[] CryptographyPasswordAs256Hash(string word)
    {
        var wordBytes = Encoding.UTF8.GetBytes(word);
        using var sha256 = SHA256.Create();

        var sha256Word = sha256.ComputeHash(wordBytes);
        return sha256Word;
    }
}