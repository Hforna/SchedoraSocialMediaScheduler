using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Schedora.Domain.Dtos;
using Schedora.Domain.Entities;
using Schedora.Domain.Exceptions;
using Schedora.Domain.Services;
using Schedora.Infrastructure.Services.ExternalServicesConfigs;

namespace Schedora.Infrastructure.Services.Externals;

public class TwitterPostService : ISocialPostService
{
    public string Platform => SocialPlatformsNames.Twitter;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITwitterConfiguration _twitterConfiguration;
    private readonly ILogger<TwitterPostService> _logger;

    public TwitterPostService(
        IServiceProvider serviceProvider,
        ITwitterConfiguration twitterConfiguration,
        ILogger<TwitterPostService> logger)
    {
        _serviceProvider = serviceProvider;
        _twitterConfiguration = twitterConfiguration;
        _logger = logger;
    }

    public async Task CreatePost(SocialCreatePostRequestDto request, SocialAccount userSocialAccount, CancellationToken cancellationToken = default)
    {
        try
        {
            var accessToken = userSocialAccount.AccessToken;
            

            using var scope = _serviceProvider.CreateScope();
            var cryptographyService = scope.ServiceProvider.GetRequiredService<ITokensCryptographyService>();
            accessToken = cryptographyService.DecryptToken(accessToken);
            var httpClient = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var payload = BuildTwitterCreatePostPayload(request);

            var url = $"{_twitterConfiguration.GetTwitterUri()}tweets";
            var response = await httpClient.PostAsJsonAsync(url, payload, cancellationToken);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Twitter create post response content: {Content}", content);

            if (!response.IsSuccessStatusCode)
                throw new ExternalServiceException("There was an error creating the Twitter post.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw new ExternalServiceException("There was an error creating the Twitter post.");
        }
    }

    private static object BuildTwitterCreatePostPayload(SocialCreatePostRequestDto request)
    {
        var text = request.Text?.Trim() ?? string.Empty;
        var mediaIds = request.MediaIds?
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct()
            .ToList() ?? new List<string>();

        if (mediaIds.Count == 0)
        {
            return new
            {
                text
            };
        }

        return new
        {
            text,
            media = new
            {
                media_ids = mediaIds
            }
        };
    }
}