using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Schedora.Domain.Dtos;
using Schedora.Domain.Entities;
using Schedora.Domain.Exceptions;
using Schedora.Domain.Services;
using Schedora.Domain.Services.Cache;
using Schedora.Infrastructure.Utils;

namespace Schedora.Infrastructure.ExternalServices;

public class LinkedInService : ILinkedInService
{
    public LinkedInService(IServiceProvider serviceProvider, ILogger<LinkedInOAuthAuthenticationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    private const string BaseLinkedInUrl = "https://api.linkedin.com/v2/";
    
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LinkedInOAuthAuthenticationService> _logger;
    
    
    public async Task<SocialAccountInfosDto> GetSocialAccountInfos(string accessToken, string tokenType)
    {
        using var scope = _serviceProvider.CreateScope();
        var httpClient = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();

        try
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenType, accessToken);

            var request = await httpClient.GetAsync($"{BaseLinkedInUrl}userinfo");
            
            request.EnsureSuccessStatusCode();

            var content = await request.Content.ReadAsStringAsync();

            var deserialize = JsonSerializer.Deserialize<LinkedInUserInfosDto>(content);
            
            if (deserialize is null)
                throw new ExternalServiceException("Failed to parse response");

            var response = new SocialAccountInfosDto()
            {
                Email = deserialize.Email,
                FirstName = deserialize.GivenName,
                LastName = deserialize.FamilyName,
                ProfileImageUrl = deserialize.ProfilePicture,
                UserId = deserialize.Id,
                UserName = deserialize.UserName
            };
            
            return response;
        }
        catch (HttpRequestException he)
        {
            _logger.LogError(he, he.Message);
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An unexpected error has occurred");
            throw;
        }
    }
}

public class LinkedInOAuthAuthenticationService : IExternalOAuthAuthenticationService
{
    public LinkedInOAuthAuthenticationService(IServiceProvider serviceProvider, ILogger<LinkedInOAuthAuthenticationService> logger, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _clientId = configuration.GetValue<string>("LinkedIn:clientId")!;
        _clientSecret = configuration.GetValue<string>("LinkedIn:clientSecret")!;
    }

    private readonly IServiceProvider  _serviceProvider;
    private readonly ILogger<LinkedInOAuthAuthenticationService> _logger;
    private readonly string _clientId;
    private readonly string _clientSecret;


    public string Platform { get; } = SocialPlatformsNames.LinkedIn;
    
    public async Task<string> GetOAuthRedirectUrl(string redirectUrl)
    {
        using var scope = _serviceProvider.CreateScope();
        var httpClient = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var cacheService = scope.ServiceProvider.GetRequiredService<ISocialAccountCache>();
        var tokenService =  scope.ServiceProvider.GetRequiredService<ITokenService>();
        
        try
        {
            var user = await tokenService.GetUserByToken();
            
            var state = GenerateStrings.GenerateRandomString(32);

            await cacheService.AddStateAuthorization(state, user.Id, SocialPlatformsNames.LinkedIn);
            
            var authorizeUrl =
                "https://www.linkedin.com/oauth/v2/authorization" +
                $"?response_type=code" +
                $"&client_id={_clientId}" +
                $"&redirect_uri={redirectUrl}" +
                $"&state={state}" +
                "&scope=openid%20profile%20email%20w_member_social";

            return authorizeUrl;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An unexpected error has occurred");
            throw;
        }
    }

    public async Task<ExternalServicesTokensDto> RequestAccessFromOAuthPlatform(string code, string redirectUrl)
    {
        using var scope = _serviceProvider.CreateScope();
        var httpClient = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();

        try
        {
            var queryParams = new Dictionary<string, string>()
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "redirect_uri", redirectUrl },
            };

            var @params = new FormUrlEncodedContent(queryParams);
            var request = await httpClient.PostAsync("https://www.linkedin.com/oauth/v2/accessToken", @params);
            request.EnsureSuccessStatusCode();

            var content = await request.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<ExternalServicesTokensDto>(content);

            if (response is null)
                throw new ExternalServiceException("Failed to parse response");

            return response;
        }
        catch (HttpRequestException he)
        {
            _logger.LogError(he, he.Message);
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An unexpected error has occurred");
            throw;
        }
    }
}