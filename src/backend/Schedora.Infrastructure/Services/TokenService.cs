using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Schedora.Domain.Entities;
using Schedora.Domain.Exceptions;
using Schedora.Domain.Interfaces;
using Schedora.Domain.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Schedora.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly ILogger<ITokenService> _logger;
    private readonly TokenValidationParameters _validationParameters;
    private readonly IRequestService _requestService;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _signKey;
    private readonly int _expiresAtMinutes;
    private readonly IUnitOfWork _uow;
    
    public TokenService(ILogger<ITokenService> logger, IRequestService requestService, 
        IServiceProvider serviceProvider, TokenValidationParameters validationParameters, string signKey,
        int expiresAtMinutes)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _requestService = requestService;
        _validationParameters = validationParameters;
        _signKey = signKey;
        _expiresAtMinutes = expiresAtMinutes;

        _uow = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IUnitOfWork>();
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

    public List<Claim>? GetTokenClaims()
    {
        var token = _requestService.GetAuthenticationHeaderToken()
                ?? throw new RequestException("Token não fornecido na requisição");

        var handler = new JwtSecurityTokenHandler();
        var result = handler.ValidateToken(token, _validationParameters, out SecurityToken validated);

        return result.Claims.ToList();
    }
    
    public async Task<User?> GetUserByToken()
    {
        var token = _requestService.GetAuthenticationHeaderToken();

        if (string.IsNullOrEmpty(token))
            return null!;

        var handler = new JwtSecurityTokenHandler();
        var read = handler.ReadJwtToken(token);
        var id = long.Parse(read.Claims.FirstOrDefault(d => d.Type == ClaimTypes.Sid)!.Value);
        var user = await _uow.UserRepository.GetUserById(id);

        return user!;
    }
}