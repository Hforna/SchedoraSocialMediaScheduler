namespace Schedora.Domain.Entities;

public class SocialAccount : Entity
{
    public long UserId { get; set;  }
    public string Platform { get; set; }
    public string PlatformUserId { get; set; }
    public string UserName { get; set; }
    public string DisplayName { get; set; }
    public string ProfileImageUrl { get; set; }
    public int FollowerCount { get; set; } = 0;
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime TokenExpiresAt { get; set; }
    public string TokenType { get; set; }
    public string Scopes { get; set; }
    public bool IsActive { get; set; }
    public DateTime LastTokenRefreshAt { get; set; }
    public DateTime LastSyncAt { get; set; }
    public DateTime ConnectedAt { get; set; }
}