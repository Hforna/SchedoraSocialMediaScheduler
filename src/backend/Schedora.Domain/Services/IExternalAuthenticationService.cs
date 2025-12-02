namespace Schedora.Domain.Services;

public interface IExternalAuthenticationService
{
    public string Platform { get; }
    public Task<string> GetOAuthRedirectUrl(string redirectUrl);
}