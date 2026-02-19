using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Schedora.Domain.Dtos;
using Schedora.Domain.Dtos.Externals;
using Schedora.Domain.Entities;
using Schedora.Domain.Exceptions;
using Schedora.Domain.Services;
using Schedora.Infrastructure.Services.ExternalServicesConfigs;

namespace Schedora.Infrastructure.Services.Externals;

public class TwitterMediaService : ISocialMediaUploaderService
{
    private readonly IUnitOfWork _uow;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITwitterConfiguration  _twitterConfiguration;
    private readonly ILogger<TwitterMediaService> _logger;

    public TwitterMediaService(IUnitOfWork uow, IServiceProvider serviceProvider, ITwitterConfiguration twitterConfiguration, ILogger<TwitterMediaService> logger)
    {
        _uow = uow;
        _serviceProvider = serviceProvider;
        _twitterConfiguration = twitterConfiguration;
        _logger = logger;
    }

    public async Task<MediaUploadDataDto> Upload(SocialAccount userSocialAccount, string mimeType, string mediaUrl)
    {
        try
        {
            var accessToken = userSocialAccount.AccessToken;
            
            using var scope = _serviceProvider.CreateScope();
            var httpClient = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
            
            var userSocialId = new List<string>() { userSocialAccount.PlatformUserId };
            var requestData = new Dictionary<string, string>()
            {
                { "media", mediaUrl },
                { "media_category", "tweet_image" },
                { "additional_owners", JsonSerializer.Serialize(userSocialId) },
                { "media_type", mimeType },
                { "shared", true.ToString() }
            };

            var response = await
                httpClient.PostAsJsonAsync($"{_twitterConfiguration.GetTwitterUri()}media/upload", requestData);

            var content = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation($"Response content: {content}");
            
            if(!response.IsSuccessStatusCode)
                throw new ExternalServiceException("There was an error uploading the media.");

            var deserialize = JsonSerializer.Deserialize<TwitterMediaUploadDto>(content);
            
            if (deserialize is null)
                throw new ExternalServiceException("Failed to parse response");

            return JsonSerializer.Deserialize<MediaUploadDataDto>(JsonSerializer.Serialize(deserialize.Data));
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            
            throw new ExternalServiceException("There was an error uploading the media.");
        }
    }
}