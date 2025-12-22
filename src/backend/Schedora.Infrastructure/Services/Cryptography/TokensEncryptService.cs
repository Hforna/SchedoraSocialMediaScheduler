using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Schedora.Domain.Services;
using Sodium;

namespace Schedora.Infrastructure.Services;

public class TokensEncryptService : ITokensCryptographyService
{
    private readonly byte[] _key;

    public TokensEncryptService(IConfiguration configuration)
    {
        _key = Convert.FromBase64String(
            configuration.GetValue<string>("services:security:cryptography:key")!);
    }

    public string EncryptToken(string token)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = _key;
            aes.GenerateIV();
            
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var tokenBytes = Encoding.UTF8.GetBytes(token);
            var encrypted = encryptor.TransformFinalBlock(tokenBytes, 0, tokenBytes.Length);
            
            var result = new byte[aes.IV.Length + encrypted.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(encrypted, 0, result, aes.IV.Length, encrypted.Length);
            
            return Convert.ToBase64String(result);
        }
    }
    
    public string DecryptToken(string encryptedToken)
    {
        var fullCipher = Convert.FromBase64String(encryptedToken);
        
        using (var aes = Aes.Create())
        {
            aes.Key = _key;
            
            var iv = new byte[aes.IV.Length];
            var cipher = new byte[fullCipher.Length - iv.Length];
            
            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);
            
            aes.IV = iv;
            
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            var decrypted = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
            
            return Encoding.UTF8.GetString(decrypted);
        }
    }
}