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
using Schedora.Infrastructure.Services.ExternalServicesConfigs;
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
                FullName = $"{deserialize.GivenName} {deserialize.FamilyName}",
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
    public LinkedInOAuthAuthenticationService(IServiceProvider serviceProvider, ILogger<LinkedInOAuthAuthenticationService> logger, 
        IConfiguration configuration, IOAuthStateService stateService, 
        ICurrentUserService  currentUserService, ILinkedInOAuthConfiguration linkedInOAuthConfiguration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _currentUserService = currentUserService;
        _linkedInOAuthConfiguration = linkedInOAuthConfiguration;
        _stateService = stateService;
        _clientId = configuration.GetValue<string>("LinkedIn:clientId")!;
        _clientSecret = configuration.GetValue<string>("LinkedIn:clientSecret")!;
    }

    private readonly IServiceProvider  _serviceProvider;
    private readonly ILogger<LinkedInOAuthAuthenticationService> _logger;
    private readonly IOAuthStateService  _stateService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILinkedInOAuthConfiguration  _linkedInOAuthConfiguration;
    private readonly string _clientId;
    private readonly string _clientSecret;


    public string Platform { get; } = SocialPlatformsNames.LinkedIn;
    
    public async Task<string> GetOAuthRedirectUrl(string redirectUrl, string callbackUrl)
    {
        using var scope = _serviceProvider.CreateScope();
        var httpClient = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        
        try
        {
            var user = await _currentUserService.GetUser();
            
            var state = GenerateStrings.GenerateRandomString(32);

            await _stateService.StorageState(state, user.Id, SocialPlatformsNames.LinkedIn, redirectUrl);
            
            var authorizeUrl =
                $"{_linkedInOAuthConfiguration.GetOAuthUri()}authorization" +
                $"?response_type=code" +
                $"&client_id={_clientId}" +
                $"&redirect_uri={callbackUrl}" +
                $"&state={state}" +
                $"&scope={_linkedInOAuthConfiguration.GetScopesAvailable()}";

            return authorizeUrl;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An unexpected error has occurred");
            throw;
        }
    }

    public async Task<ExternalServicesTokensDto> RequestTokensFromOAuthPlatform(string code, string redirectUrl, string codeVerifier = "")
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
            var request =
                await httpClient.PostAsync($"{_linkedInOAuthConfiguration.GetOAuthUri()}accessToken", @params);
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
        catch (ExternalServiceException ex)
        {
            _logger.LogError(ex, ex.Message);

            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An unexpected error has occurred");
            throw;
        }
    }
}