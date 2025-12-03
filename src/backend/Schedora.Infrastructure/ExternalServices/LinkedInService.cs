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

namespace Schedora.Infrastructure.ExternalServices;

public class LinkedInService : ILinkedInService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LinkedInOAuthAuthenticationService> _logger;
    
    public Task<SocialAccountInfosDto> GetSocialAccountInfos(string accessToken)
    {
        throw new NotImplementedException();
    }
}

public class LinkedInOAuthAuthenticationService : IExternalOAuthAuthenticationService
{
    public LinkedInOAuthAuthenticationService(IServiceProvider serviceProvider, ILogger<LinkedInOAuthAuthenticationService> logger, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _clientId = configuration.GetValue<string>("LinkedIn:ClientId")!;
        _clientSecret = configuration.GetValue<string>("LinkedIn:ClientSecret")!;
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

        try
        {
            var authorizeUrl =
                "https://www.linkedin.com/oauth/v2/authorization" +
                $"?response_type=code" +
                $"&client_id={_clientId}" +
                $"&redirect_uri={redirectUrl}" +
                "&state=foobar" +
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
            httpClient.DefaultRequestHeaders.Add("Content-Type", "x-www-form-urlencoded");

            var queryParams = new Dictionary<string, string>()
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", redirectUrl },
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
            };

            var url = QueryHelpers.AddQueryString("https://www.linkedin.com/in/oauth/v2/authorization", queryParams!);
            var request = await httpClient.PostAsync(url, null);

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