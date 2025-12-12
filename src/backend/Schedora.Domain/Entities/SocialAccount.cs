using Schedora.Domain.Exceptions;

namespace Schedora.Domain.Entities;

public class SocialAccount : Entity
{
    public long UserId { get; set;  }
    public string Platform { get; set; }
    public string PlatformUserId { get; set; }
    public string UserName { get; set; }
    public string? ProfileImageUrl { get; set; }
    public int FollowerCount { get; set; } = 0;
    public string AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime TokenExpiresAt { get; set; }
    public string TokenType { get; set; }
    public string Scopes { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastTokenRefreshAt { get; set; }
    public DateTime? LastSyncAt { get; set; } = DateTime.UtcNow;
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;

    public static SocialAccount Create(long userId, string platform, string platformUserId, 
        string userName, string tokenType, string scopes, string accessToken, string refreshToken, DateTime tokenExpiresAt)
    {
        if (string.IsNullOrEmpty(platform) || !SocialPlatformsNames.GetAllConsts().Contains(platform))
            throw new DomainException("Invalid platform name");
        
        return new()
        {
            UserId = userId,
            Platform = platform,
            PlatformUserId = platformUserId,
            UserName = userName,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenExpiresAt = tokenExpiresAt,
            TokenType = tokenType,
            Scopes = scopes,
            IsActive = true,
            LastTokenRefreshAt = DateTime.UtcNow,
            LastSyncAt = DateTime.UtcNow
        };
    }
}

public static class SocialPlatformsNames
{
    public const string Twitter = "Twitter";
    public const string LinkedIn = "LinkedIn";

    public static string NormalizePlatform(string platform)
    {
        platform =  GetAllConsts().FirstOrDefault(d => d.Equals(platform, StringComparison.OrdinalIgnoreCase)) 
                    ?? throw new RequestException("Invalid social platform name");
        
        return platform;
    }

    public static List<string> GetAllConsts()
    {
        var values = typeof(SocialPlatformsNames)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.IsLiteral)
            .Select(f => f.GetValue(null).ToString())
            .ToList();
        
        return values;
    }
}