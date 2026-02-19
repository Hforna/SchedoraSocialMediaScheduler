using Schedora.Domain.Dtos;

namespace Schedora.Domain.Services;

public interface IExternalSocialAccountService
{
    public string Platform { get; }
    public Task<SocialAccountInfosDto> GetSocialAccountInfos(string accessToken, string tokenType);
}