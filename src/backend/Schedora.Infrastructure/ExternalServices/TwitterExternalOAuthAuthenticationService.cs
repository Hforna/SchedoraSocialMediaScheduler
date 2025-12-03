using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Schedora.Domain.Dtos;
using Schedora.Domain.Entities;
using Schedora.Domain.Exceptions;
using Schedora.Domain.Services;

namespace Schedora.Infrastructure.ExternalServices;

public class TwitterExternalOAuthAuthenticationService : IExternalOAuthAuthenticationService
{
    public string Platform { get; } = SocialPlatformsNames.Twitter;
    
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TwitterExternalOAuthAuthenticationService> _logger;
    private readonly ICryptographyService _cryptographyService;
    private readonly string _clientId;
    
    public TwitterExternalOAuthAuthenticationService(IServiceProvider serviceProvider, ILogger<TwitterExternalOAuthAuthenticationService> logger, 
        IConfiguration configuration, ICryptographyService cryptographyService)
    {

        _serviceProvider = serviceProvider;
        _logger = logger;
        _cryptographyService = cryptographyService;
        
        string clientId = configuration.GetValue<string>("Twitter:ClientId");
        string consumerSecret = configuration.GetValue<string>("Twitter:ConsumerSecret");

        _clientId = clientId;
    }
    

    public async Task<string> GetOAuthRedirectUrl(string uri)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            using var client = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();

            var state = GenerateRandomString(32);
            var codeChallenge = GenerateCodeChallenge();

            var scopes = "tweet.read users.read account.follows.read account.follows.write";
            
            return $"https://x.com/i/oauth2/authorize?" +
                                               $"response_type=code&client_id={_clientId}&" +
                                               $"redirect_uri={Uri.EscapeDataString(uri)}&" +
                                               $"scope={Uri.EscapeDataString(scopes)}&" +
                                               $"state={state}&" +
                                               $"code_challenge={codeChallenge}&" +
                                               $"code_challenge_method=S256";

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            
            throw;
        }
    }

    public Task<ExternalServicesTokensDto> RequestAccessFromOAuthPlatform(string code, string redirectUrl)
    {
        throw new NotImplementedException();
    }

    private string GenerateCodeChallenge()
    {
        var randomString = GenerateRandomString(128);
        var stringAs256 = _cryptographyService.CryptographyPasswordAs256Hash(randomString);
        
        return Base64UrlEncode(stringAs256);
    }

    private string Base64UrlEncode(byte[] data)
    {
        var base64 = Convert.ToBase64String(data);
        
        return base64
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static string GenerateRandomString(int length)
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