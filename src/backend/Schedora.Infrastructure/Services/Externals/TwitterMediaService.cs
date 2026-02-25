using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Schedora.Domain.Dtos;
using Schedora.Domain.Dtos.Externals;
using Schedora.Domain.Entities;
using Schedora.Domain.Enums;
using Schedora.Domain.Exceptions;
using Schedora.Domain.Services;
using Schedora.Infrastructure.Services.ExternalServicesConfigs;

namespace Schedora.Infrastructure.Services.Externals;

public interface ITwitterMediaService
{
    public Task<MediaUploadDataDto> GetMediaUploadStatus(string mediaId, SocialAccount userSocialAccount);

    public Task<MediaUploadDataDto> WaitForMediaProcessingAsync(
        string mediaId,
        SocialAccount userSocialAccount,
        CancellationToken cancellationToken,
        int maxAttempts = 20,
        int defaultCheckAfterSeconds = 5);
}

public class TwitterMediaService : ISocialMediaUploaderService, ITwitterMediaService
{
    public string Platform { get; }= SocialPlatformsNames.Twitter;
    private readonly IUnitOfWork _uow;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITwitterConfiguration _twitterConfiguration;
    private readonly ITokensCryptographyService _tokensCryptography;
    private readonly ILogger<TwitterMediaService> _logger;

    public TwitterMediaService(
        IUnitOfWork uow,
        IServiceProvider serviceProvider,
        ITwitterConfiguration twitterConfiguration,
        ITokensCryptographyService tokensCryptography,
        ILogger<TwitterMediaService> logger)
    {
        _uow = uow;
        _serviceProvider = serviceProvider;
        _twitterConfiguration = twitterConfiguration;
        _tokensCryptography = tokensCryptography;
        _logger = logger;
    }

    public async Task<MediaUploadDataDto> Upload(SocialAccount userSocialAccount, Media media, string mediaUrl)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(mediaUrl))
                throw new ExternalServiceException("MediaUrl is required.");

            if (string.IsNullOrWhiteSpace(media.MimeType))
                throw new ExternalServiceException("MimeType is required.");

            var accessToken = _tokensCryptography.DecryptToken(userSocialAccount.AccessToken);

            using var scope = _serviceProvider.CreateScope();
            var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            
            var twitterClient = httpClientFactory.CreateClient();
            twitterClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            
            var downloadClient = httpClientFactory.CreateClient();

            var mediaBytes = await downloadClient.GetByteArrayAsync(mediaUrl);
            if (mediaBytes.Length == 0)
                throw new ExternalServiceException("Downloaded media is empty.");

            var uploadEndpoint = $"{_twitterConfiguration.GetTwitterUri()}media/upload/";

            var response = mediaBytes.Length > 5 * 1024 * 1024 
                ? await UploadMediaGreatherThan5Mb(twitterClient, mediaUrl, media, userSocialAccount, mediaBytes, uploadEndpoint) 
                : await UploadMediaLessThan5Mb(media, userSocialAccount.PlatformUserId, twitterClient, mediaBytes, uploadEndpoint);;

            var data = response.Data;
            return new MediaUploadDataDto
            {
                ExpiresAfterSecs = data.ExpiresAfterSecs,
                Id = data.Id,
                MediaKey = data.MediaKey,
                Size = data.Size,
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw new ExternalServiceException("There was an error uploading the media.");
        }
    }

    private async Task<TwitterMediaUploadDto> InitMedia(Media media, string userId, HttpClient client, byte[] mediaBytes, string uploadEndpoint)
    {
        var initForm = new Dictionary<string, object>
        {
            ["total_bytes"] = mediaBytes.Length,
            ["media_type"] = "video/mp4",
            ["media_category"] = "tweet_video",//media.Type == MediaType.IMAGE ? "tweet_image" : "tweet_video",
            ["additional_owners"] = new List<string>(){userId}
        };

        var initResponse = await client.PostAsJsonAsync($"{uploadEndpoint}initialize", initForm);
        var initContent = await initResponse.Content.ReadAsStringAsync();
        _logger.LogInformation("Twitter media INIT response content: {Content}", initContent);

        if (!initResponse.IsSuccessStatusCode)
            throw new ExternalServiceException("There was an error initializing the media upload.");

        var response = JsonSerializer.Deserialize<TwitterMediaUploadDto>(initContent);
        
        if(string.IsNullOrEmpty(response.Data.Id))
            throw new ExternalServiceException("Failed to parse INIT response.");
        
        return response;
    }

    private async Task AppendMedia(HttpClient twitterClient, byte[] mediaBytes, string uploadEndpoint, string mediaId)
    {
        const int chunkSize = 4 * 1024 * 1024; // 4MB chunks (safe default)
        var segmentIndex = 0;

        for (var offset = 0; offset < mediaBytes.Length; offset += chunkSize)
        {
            var count = Math.Min(chunkSize, mediaBytes.Length - offset);

            var chunk = mediaBytes.AsSpan(offset, count).ToArray();
            var multipart = new MultipartFormDataContent();
            multipart.Add(new ByteArrayContent(chunk), "media");
            multipart.Add(new StringContent(segmentIndex.ToString()), "segment_index");
            await twitterClient.PostAsync($"{uploadEndpoint}{mediaId}/append", multipart);

            var appendResponse = await twitterClient.PostAsync($"{uploadEndpoint}{mediaId}/append", multipart);
            var appendBody = await appendResponse.Content.ReadAsStringAsync();

            if (!appendResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation("Twitter media APPEND response content: {Content}", appendBody);
                throw new ExternalServiceException($"There was an error appending media segment {segmentIndex}.");
            }

            segmentIndex++;
        }
    }

    private async Task<TwitterMediaUploadDto> FinalizeMediaUploading(HttpClient twitterClient, string mediaId, string uploadEndpoint)
    {
        var finalizeResponse = await twitterClient.PostAsync($"{uploadEndpoint}{mediaId}/finalize", null);
        var finalizeContent = await finalizeResponse.Content.ReadAsStringAsync();
        _logger.LogInformation("Twitter media FINALIZE response content: {Content}", finalizeContent);

        if (!finalizeResponse.IsSuccessStatusCode)
            throw new ExternalServiceException("There was an error finalizing the media upload.");

        var finalizeDto = JsonSerializer.Deserialize<TwitterMediaUploadDto>(finalizeContent);
        
        return finalizeDto?.Data is null ? throw new ExternalServiceException("Failed to parse FINALIZE response.") : finalizeDto;
    }

    private async Task<TwitterMediaUploadDto> UploadMediaGreatherThan5Mb(HttpClient twitterClient, string mediaUrl, 
        Media media, SocialAccount userSocialAccount, byte[] mediaBytes, string uploadEndpoint)
    {
        var initResponse = await InitMedia(media, userSocialAccount.PlatformUserId, twitterClient, mediaBytes, uploadEndpoint);

        await AppendMedia(twitterClient, mediaBytes, uploadEndpoint, initResponse.Data.Id);
        
        var finalizeResponse = await FinalizeMediaUploading(twitterClient, initResponse.Data.Id, uploadEndpoint);
        
        return finalizeResponse;
    }

    private async Task<TwitterMediaUploadDto> UploadMediaLessThan5Mb(Media media, string userId, HttpClient client, byte[] mediaBytes, string uploadEndpoint)
    {
        var multipart = new MultipartFormDataContent();
        multipart.Add(new ByteArrayContent(mediaBytes), "media");
        multipart.Add(new StringContent(media.MimeType), "media_type");
        multipart.Add(new StringContent(media.Type == MediaType.IMAGE ? "tweet_image" : "tweet_video"), "media_category");
        multipart.Add(new StringContent(userId), "additional_owners");
        multipart.Add(new StringContent(false.ToString()), "shared");
        
        var response = await client.PostAsync($"{uploadEndpoint}media/upload", multipart);
        var content = await response.Content.ReadAsStringAsync();
        
        response.EnsureSuccessStatusCode();
        
        _logger.LogInformation("Twitter media INIT response content: {Content}", content);
        
        return JsonSerializer.Deserialize<TwitterMediaUploadDto>(content);
    }

    public async Task<MediaUploadDataDto> GetMediaUploadStatus(string mediaId, SocialAccount userSocialAccount)
    {
        try
        {
            var accessToken = _tokensCryptography.DecryptToken(userSocialAccount.AccessToken);

            using var scope = _serviceProvider.CreateScope();
            var httpClient = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var url = $"{_twitterConfiguration.GetTwitterUri()}media/upload?command=STATUS&media_id={mediaId}";
            var uploadResponse = await httpClient.GetAsync(url);

            var content = await uploadResponse.Content.ReadAsStringAsync();
            _logger.LogInformation("Twitter media status response content: {Content}", content);

            if (!uploadResponse.IsSuccessStatusCode)
                throw new ExternalServiceException("There was an error getting the media upload status.");

            var deserialize = JsonSerializer.Deserialize<TwitterMediaUploadDto>(content);
            if (deserialize?.Data is null)
                throw new ExternalServiceException("Failed to parse media upload status response.");

            var serialize = JsonSerializer.Serialize(deserialize.Data);
            var response = JsonSerializer.Deserialize<MediaUploadDataDto>(serialize);

            return new MediaUploadDataDto
            {
                ExpiresAfterSecs = deserialize.Data.ExpiresAfterSecs,
                Id = deserialize.Data.Id,
                MediaKey = deserialize.Data.MediaKey,
                Size = deserialize.Data.Size,
                ProcessingInfo = deserialize.Data.ProcessingInfo == null ? null : new MediaProcessingInfoDto
                {
                    CheckAfterSecs = deserialize.Data.ProcessingInfo.CheckAfterSecs,
                    ProgressPercent = deserialize.Data.ProcessingInfo.ProgressPercent,
                    State = deserialize.Data.ProcessingInfo.State
                }
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw new ExternalServiceException("There was an error getting the media upload status.");
        }
    }

    public async Task<MediaUploadDataDto> WaitForMediaProcessingAsync(
        string mediaId,
        SocialAccount userSocialAccount,
        CancellationToken cancellationToken,
        int maxAttempts = 20,
        int defaultCheckAfterSeconds = 5)
    {
        if (string.IsNullOrWhiteSpace(mediaId))
            throw new ExternalServiceException("MediaId is required to check processing status.");

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var status = await GetMediaUploadStatus(mediaId, userSocialAccount);

            var state = status.ProcessingInfo?.State;
            
            if (string.IsNullOrWhiteSpace(state))
                return status;

            if (string.Equals(state, "succeeded", StringComparison.OrdinalIgnoreCase))
                return status;

            if (string.Equals(state, "failed", StringComparison.OrdinalIgnoreCase))
                throw new ExternalServiceException($"Twitter media processing failed for mediaId '{mediaId}'.");

            if (!string.Equals(state, "pending", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(state, "in_progress", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Unknown media processing state '{State}' for mediaId '{MediaId}'. Returning last status.", state, mediaId);
                return status;
            }

            var delaySeconds = status.ProcessingInfo?.CheckAfterSecs > 0
                ? status.ProcessingInfo.CheckAfterSecs
                : defaultCheckAfterSeconds;

            _logger.LogInformation(
                "MediaId '{MediaId}' still processing (state: {State}, attempt {Attempt}/{MaxAttempts}). Next check in {DelaySeconds}s.",
                mediaId, state, attempt, maxAttempts, delaySeconds);

            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
        }

        throw new ExternalServiceException($"Timed out waiting for Twitter media processing for mediaId '{mediaId}'.");
    }
}