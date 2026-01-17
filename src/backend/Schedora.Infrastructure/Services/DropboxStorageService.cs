using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.Extensions.Logging;
using Schedora.Domain.Services;

namespace Schedora.Infrastructure.Services;

public class DropboxStorageService : IStorageService
{
    private readonly string _accessToken;
    private readonly ILogger<DropboxStorageService> _logger;
    private const string BaseFilePath = "/uploads/files/";
    
    public DropboxStorageService(string accessToken, ILogger<DropboxStorageService> logger)
    {
        _accessToken = accessToken;
        _logger = logger;
    }
    
    public async Task UploadMedia(Stream media, string fileName, long userId)
    {
        using var client = new DropboxClient(_accessToken);
        var result = await client.Files.UploadAsync(
            path: $"{BaseFilePath}{fileName}", 
            mode: WriteMode.Overwrite.Instance,
            body: media);
    }

    public async Task<string> GetUrlMedia(string fileName, long userId)
    {
        using var client = new DropboxClient(_accessToken);
        try
        {
            var link = await client.Files.GetTemporaryLinkAsync($"{BaseFilePath}{fileName}");
                
            _logger.LogInformation($"Getting file link: {link}");

            return link.Link.Replace("?dl=0", "?raw=1");
        }
        catch (Exception ex)
        {
            return "";
        }
    }
}

public class TestClass : IStorageService
{
    public async Task UploadMedia(Stream media, string fileName, long userId)
    {
        
    }

    public async Task<string> GetUrlMedia(string fileName, long userId)
    {
        return "random url media";
    }
}