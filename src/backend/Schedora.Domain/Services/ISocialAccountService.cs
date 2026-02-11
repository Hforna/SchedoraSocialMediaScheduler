using Schedora.Domain.Dtos;

namespace Schedora.Domain.Services;

public interface ISocialAccountService
{
    public string Platform { get; }
    public Task<SocialAccountInfosDto> GetSocialAccountInfos(string accessToken, string tokenType);
}