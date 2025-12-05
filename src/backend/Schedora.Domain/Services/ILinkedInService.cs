using Schedora.Domain.Dtos;

namespace Schedora.Domain.Services;

public interface ILinkedInService
{
    public Task<SocialAccountInfosDto> GetSocialAccountInfos(string accessToken, string tokenType);
}