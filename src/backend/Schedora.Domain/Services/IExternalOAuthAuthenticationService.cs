using Schedora.Domain.Dtos;

namespace Schedora.Domain.Services;

public interface IExternalOAuthAuthenticationService
{
    public string Platform { get; }
    public Task<string> GetOAuthRedirectUrl(string redirectUrl);
    public Task<ExternalServicesTokensDto> RequestTokensFromOAuthPlatform(string code, string redirectUrl, string codeVerifier = "");
}