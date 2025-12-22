using System.Net.Http.Headers;
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

public class TwitterService : ITwitterService
{
    public TwitterService(ILogger<TwitterService> logger, IServiceProvider serviceProvider, ITwitterConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    private readonly ILogger<TwitterService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITwitterConfiguration _configuration;
    
    public async Task<SocialAccountInfosDto> GetUserSocialAccountInfos(string accessToken, string tokenType)
    {
        using var scope = _serviceProvider.CreateScope();
        var httpClient = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();
        
        if(tokenType.Equals("Bearer",  StringComparison.InvariantCultureIgnoreCase))
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        var request = await httpClient
            .GetAsync($"{_configuration.GetTwitterUri()}users/me?user.fields=created_at,description,location,profile_image_url,public_metrics");

        var content = await request.Content.ReadAsStringAsync();

        _logger.LogInformation("Content response from request {content}", content);
        
        var deserialize = JsonSerializer.Deserialize<TwitterUserInfosData>(content);
        
        var dto =  new SocialAccountInfosDto()
        {
            UserId = deserialize!.Data.Id,
            UserName =  deserialize.Data.Username,
            Email =  "",
            FullName = deserialize.Data.Name,
            PictureUrl = deserialize.Data.ProfileImageUrl,
            FollowersCount = deserialize.Data.PublicMetrics.FollowersCount,
            FollowingCount = deserialize.Data.PublicMetrics.FollowingCount,
        };

        return dto;
    }
}

public class TwitterExternalOAuthAuthenticationService : IExternalOAuthAuthenticationService, IOAuthTokenService
{
    public string Platform { get; } = SocialPlatformsNames.Twitter;
    
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TwitterExternalOAuthAuthenticationService> _logger;
    private readonly IPkceService  _pkceService;
    private readonly ICurrentUserService  _currentUserService;
    private readonly ITwitterOAuthConfiguration _oauthConfig;
    private readonly IOAuthStateService _oauthStateService;
    private readonly string _clientId;
    private readonly string _clientSecret;
    
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
        string clientSecret = configuration.GetValue<string>("Twitter:ClientSecret");

        _clientSecret = clientSecret;
        _clientId = clientId;
    }
    

    public async Task<string> GetOAuthRedirectUrl(string redirectUri, string callbackUrl)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            using var client = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();
            var cacheService = scope.ServiceProvider.GetRequiredService<ISocialAccountCache>();

            var user = await _currentUserService.GetUser();
            
            var state = GenerateStrings.GenerateRandomString(32);

            await _oauthStateService.StorageState(state, user!.Id, SocialPlatformsNames.Twitter, redirectUri);
            
            var codeChallenge = _pkceService.GenerateCodeChallenge();
            await _pkceService.StorageCodeChallenge(codeChallenge.codeVerfier, user.Id,  SocialPlatformsNames.Twitter);

            var scopes = _oauthConfig.GetScopesAvailable();
            
            return $"{_oauthConfig.GetTwitterOAuthAuthorizeUri()}authorize?" +
                                               $"response_type=code" +
                                               $"&client_id={_clientId}&" +
                                               $"redirect_uri={Uri.EscapeDataString(callbackUrl)}&" +
                                               $"scope={Uri.EscapeDataString(scopes)}&" +
                                               $"state={state}&" +
                                               $"code_challenge={codeChallenge.codeChallenge}&" +
                                               $"code_challenge_method=S256";

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            
            throw;
        }
    }

    public async Task<ExternalServicesTokensDto> RequestTokensFromOAuthPlatform(string code, string redirectUrl, string codeVerifier = "")
    {
        using var scope = _serviceProvider.CreateScope();
        var httpClient  = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();
        
        var authorizationCode = $"{_clientId}:{_clientSecret}";
        var getAuthorizationBytes = Encoding.UTF8.GetBytes(authorizationCode);
        var base64 = Convert.ToBase64String(getAuthorizationBytes);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64);
        
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.x.com/2/oauth2/token");
        tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string,string>
        {
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", redirectUrl },
            { "client_id", _clientId },
            { "code_verifier", codeVerifier }
        });
        
        var url = _oauthConfig.GetTwitterOAuthUri() + "token";
        
        try
        {
            var request = await httpClient.SendAsync(tokenRequest);
            
            var content = await request.Content.ReadAsStringAsync();
            
            request.EnsureSuccessStatusCode();
        
            var deserialize = JsonSerializer.Deserialize<ExternalServicesTokensDto>(content);

            return deserialize!;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            
            throw;
        }
    }

    public async Task<ExternalServicesTokensDto> RefreshToken(string refreshToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var httpClient = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();
        
        try
        {
            var authorization = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorization);
            
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_oauthConfig.GetTwitterOAuthUri()}token");
            requestMessage.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken },
            });
            
            var request = await httpClient.SendAsync(requestMessage);
            
            var content = await request.Content.ReadAsStringAsync();
            
            request.EnsureSuccessStatusCode();
            
            var deserialize = JsonSerializer.Deserialize<ExternalServicesTokensDto>(content);
            
            return deserialize!;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            
            throw;
        }
    }
}

public interface IPkceService
{
    public (string codeChallenge, string codeVerfier) GenerateCodeChallenge();
    public Task StorageCodeChallenge(string code, long userId, string platform);
}

public class PkceService : IPkceService
{
    private readonly ICryptographyService _cryptographyService;
    private readonly ISocialAccountCache _socialAccountCache;

    public PkceService(ICryptographyService cryptographyService, ISocialAccountCache socialAccountCache)
    {
        _cryptographyService = cryptographyService;
        _socialAccountCache = socialAccountCache;
    }

    public (string codeChallenge, string codeVerfier) GenerateCodeChallenge()
    {
        var randomString = GenerateStrings.GenerateRandomString(128);
        var stringAs256 = _cryptographyService.CryptographyPasswordAs256Hash(randomString);
        
        return (Base64UrlEncode(stringAs256), randomString);
    }

    public async Task StorageCodeChallenge(string code, long userId, string platform)
    {
        await _socialAccountCache.AddCodeChallenge(code, userId, platform);
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