using Schedora.Domain.Dtos;

namespace Schedora.Domain.Services;

public interface ISocialMediaUploaderService
{
    public string Platform { get; }
    public Task<MediaUploadDataDto> Upload(SocialAccount userSocialAccount, Media media, string mediaUrl);
    public Task<MediaUploadDataDto> GetMediaUploadStatus(string mediaId, SocialAccount userSocialAccount);
}