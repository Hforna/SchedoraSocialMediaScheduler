namespace Schedora.Application.Responses;

public class UsageLimitsResponse
{
    public List<UsageLimitsConnectedAccountsResponse> ConnectedAccounts { get; set; }
    public long TotalStorageMb { get; set; }
    public long LimitStorageMb { get; set; }
    public DateTime? MediaRetentionEndsAt { get; set; }
}

public class UsageLimitsConnectedAccountsResponse
{
    public string Platform { get; set; }
    public int SocialAccountsConnected { get; set; }
    public int SocialAccountsLimit { get; set; }
}