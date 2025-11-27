using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Schedora.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Schedora.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly ILogger<ITokenService> _logger;
    private readonly TokenValidationParameters _validationParameters;
    private readonly string _signKey;
    private readonly int _expiresAtMinutes;
    
    public TokenService(ILogger<ITokenService> logger, TokenValidationParameters validationParameters, string signKey,
        int expiresAtMinutes)
    {
        _logger = logger;
        _validationParameters = validationParameters;
        _signKey = signKey;
        _expiresAtMinutes = expiresAtMinutes;
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        RandomNumberGenerator.Fill(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public DateTime GenerateRefreshTokenExpiration() => DateTime.UtcNow.AddDays(30);

    public (string token, DateTime expiresAt) GenerateToken(long userId, string userName, List<Claim>? claims = null)
    {
        claims ??= new List<Claim>();
        
        claims.Add(new Claim(ClaimTypes.Sid, userId.ToString()));
        claims.Add(new Claim(ClaimTypes.Name, userName));

        var expiration = DateTime.UtcNow.AddMinutes(_expiresAtMinutes);
        var descriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signKey)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();

        var token = handler.CreateToken(descriptor);

        return (handler.WriteToken(token), expiration);
    }
}