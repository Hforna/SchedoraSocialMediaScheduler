using System.Security.Cryptography;
using System.Text;

namespace Schedora.Infrastructure.Utils;

public static class GenerateStrings
{
    public static string GenerateRandomString(int length)
    {
        const string acceptedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~";
        
        var random = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(random);
        }
        
        var sb = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            sb.Append(acceptedChars[random[i] % acceptedChars.Length]);
        }

        return sb.ToString();
    }
}