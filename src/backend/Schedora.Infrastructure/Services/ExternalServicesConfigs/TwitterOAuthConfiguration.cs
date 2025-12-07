namespace Schedora.Infrastructure.Services.ExternalServicesConfigs;

public interface ITwitterOAuthConfiguration
{
    public string GetTwitterOAuthUri();
    public string GetScopesAvailable();
}

public class TwitterOAuthConfiguration : ITwitterOAuthConfiguration
{
    public string GetTwitterOAuthUri()
    {
        return "https://x.com/i/oauth2/";
    }

    public string GetScopesAvailable()
    {
        return "tweet.read users.read offline.access";
    }
}

public interface ITwitterConfiguration
{
    public string GetTwitterUri();
}

public class TwitterConfiguration : ITwitterConfiguration
{
    public string GetTwitterUri()
    {
        return "https://api.twitter.com/2/";
    }
}