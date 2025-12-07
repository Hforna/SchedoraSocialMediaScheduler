using Schedora.Domain.Dtos;

namespace Schedora.Domain.Services;

public interface ITwitterService
{
    public Task<SocialAccountInfosDto> GetUserSocialAccountInfos(string accessToken, string tokenType);
}