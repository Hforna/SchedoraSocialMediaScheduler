using Schedora.Domain.Dtos;

namespace Schedora.Domain.Services;

public interface ISocialMediaUploaderService
{
    public Task<MediaUploadDataDto> Upload(SocialAccount userSocialAccount, string mimeType, string mediaUrl);
}