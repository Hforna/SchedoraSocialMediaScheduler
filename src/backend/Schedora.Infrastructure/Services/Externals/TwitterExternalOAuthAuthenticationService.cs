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
using Schedora.Domain.Services.Cache;
using Schedora.Infrastructure.Services.ExternalServicesConfigs;
using Schedora.Infrastructure.Utils;

namespace Schedora.Infrastructure.ExternalServices;

public class TwitterExternalOAuthAuthenticationService : IExternalOAuthAuthenticationService
{
    public string Platform { get; } = SocialPlatformsNames.Twitter;
    
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TwitterExternalOAuthAuthenticationService> _logger;
    private readonly IPkceService  _pkceService;
    private readonly ICurrentUserService  _currentUserService;
    private readonly ITwitterOAuthConfiguration _oauthConfig;
    private readonly IOAuthStateService _oauthStateService;
    private readonly string _clientId;
    
    public TwitterExternalOAuthAuthenticationService(IServiceProvider serviceProvider, ILogger<TwitterExternalOAuthAuthenticationService> logger, 
        IConfiguration configuration, IPkceService pkceService, 
        ICurrentUserService currentUserService, ITwitterOAuthConfiguration oauthConfig, IOAuthStateService oauthStateService)
    {

        _serviceProvider = serviceProvider;
        _oauthStateService  = oauthStateService;
        _logger = logger;
        _currentUserService = currentUserService;
        _pkceService = pkceService;
        _oauthConfig = oauthConfig;
        
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
            var cacheService = scope.ServiceProvider.GetRequiredService<ISocialAccountCache>();

            var user = await _currentUserService.GetCurrentUser();
            
            var state = GenerateStrings.GenerateRandomString(32);

            await _oauthStateService.StorageState(state, user!.Id, SocialPlatformsNames.Twitter);
            
            var codeChallenge = _pkceService.GenerateCodeChallenge();

            var scopes = _oauthConfig.GetScopesAvailable();
            
            return $"{_oauthConfig.GetTwitterOAuthUri()}authorize?" +
                                               $"response_type=code" +
                                               $"&client_id={_clientId}&" +
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
    
}

public interface IPkceService
{
    public string GenerateCodeChallenge();
}

public class PkceService : IPkceService
{
    private readonly ICryptographyService _cryptographyService;

    public string GenerateCodeChallenge()
    {
        var randomString = GenerateStrings.GenerateRandomString(128);
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
}