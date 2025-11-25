using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Schedora.Domain.Interfaces;

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

    public string GenerateToken(long userId, string userName, List<Claim>? claims = null)
    {
        claims ??= new List<Claim>();
        
        claims.Add(new Claim(ClaimTypes.Sid, userId.ToString()));
        claims.Add(new Claim(ClaimTypes.Name, userName));

        var descriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_expiresAtMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signKey)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();

        var token = handler.CreateToken(descriptor);

        return handler.WriteToken(token);
    }
}