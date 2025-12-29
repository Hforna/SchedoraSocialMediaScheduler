namespace Schedora.Application.Responses;

public class SocialAccountResponse
{
    public long Id  { get; set; }
    public string Platform { get; set; }
    public string PlatformUserId { get; set; }
    public string UserName { get; set; }
    public string? ProfileImageUrl { get; set; }
    public int FollowerCount { get; set; } = 0;
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
}

public class SocialAccountsResponse
{
    public List<SocialAccountResponse> SocialAccounts { get; set; }
}