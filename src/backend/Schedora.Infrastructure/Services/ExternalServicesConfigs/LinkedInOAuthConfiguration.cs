using Microsoft.Identity.Client;

namespace Schedora.Infrastructure.Services.ExternalServicesConfigs;

public interface ILinkedInOAuthConfiguration
{
    public string GetOAuthUri();
    public string GetScopesAvailable();
}

public class LinkedInOAuthConfiguration : ILinkedInOAuthConfiguration
{
    public string GetOAuthUri()
    {
        return "https://www.linkedin.com/oauth/v2/";
    }

    public string GetScopesAvailable()
    {
        return "openid%20profile%20email%20w_member_social";
    }
}