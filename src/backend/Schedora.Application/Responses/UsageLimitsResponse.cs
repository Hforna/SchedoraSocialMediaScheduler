namespace Schedora.Application.Responses;

public class UsageLimitsResponse
{
    public int SocialAccountsConnected { get; set; }
    public int SocialAccountsLimit { get; set; }
    public long TotalStorageMb { get; set; }
    public long LimitStorageMb { get; set; }
    public int TotalAnalyzesCompleted { get; set; }
    public long LimitAnalyzesCompleted { get; set; }
    public int TotalPostsConcurrent { get; set; }
    public long LimitPostsConcurrent { get; set; }
    public DateTime MediaRetentionEndsAt { get; set; }
}