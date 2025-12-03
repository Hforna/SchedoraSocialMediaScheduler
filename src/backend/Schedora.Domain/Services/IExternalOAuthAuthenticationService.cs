using Schedora.Domain.Dtos;

namespace Schedora.Domain.Services;

public interface IExternalOAuthAuthenticationService
{
    public string Platform { get; }
    public Task<string> GetOAuthRedirectUrl(string redirectUrl);
    public Task<ExternalServicesTokensDto> RequestAccessFromOAuthPlatform(string code, string redirectUrl);
}