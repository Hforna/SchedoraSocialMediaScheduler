using System.Security.Cryptography;
using System.Text;
using Schedora.Domain.Services;

namespace Schedora.Infrastructure.Services;

public class CryptographyService : ICryptographyService
{
    public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool ValidateHash(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
    public string CryptographyPasswordAs256Hash(string word)
    {
        var wordBytes = Encoding.UTF8.GetBytes(word);
        using var sha256 = SHA256.Create();

        var sha256Word = sha256.ComputeHash(wordBytes);

        var sb = new StringBuilder();

        for (var i = 0; i < sha256Word.Length; i++)
        {
            sb.Append(sha256Word[i].ToString("x2"));
        }
        
        return sb.ToString();
    }
}