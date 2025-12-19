using Schedora.Domain.Exceptions;

namespace Schedora.Domain.Entities;

public class Subscription : IEntity
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }
    public string GatewayProvider { get; set; } = string.Empty;
    public string GatewaySubscriptionId { get; set; } = string.Empty;
    public string GatewayCustomerId { get; set; } = string.Empty;
    public string GatewayPriceId { get; set; } = string.Empty;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
    public SubscriptionEnum SubscriptionTier { get; set; }
    public DateTime? CurrentPeriodEndsAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CanceledAt { get; set; }

    public Subscription(long userId, SubscriptionEnum subscriptionTier, string? gatewayProvider,
        SubscriptionStatus status,
        string? gatewayCustomerId, string? gatewayPriceId, string? gatewaySubscriptionId, DateTime? currentPeriodEndsAt, DateTime createdAt)
    {
        UserId = userId;
        SubscriptionTier = subscriptionTier;
        GatewayProvider = gatewayProvider;
        GatewayCustomerId = gatewayCustomerId;
        GatewayPriceId = gatewayPriceId;
        GatewaySubscriptionId = gatewaySubscriptionId;
        Status = status;
        CurrentPeriodEndsAt = currentPeriodEndsAt;
        CreatedAt = createdAt;
    }

    public Subscription(long userId, SubscriptionEnum subscriptionTier)
    {
        UserId = userId;
        SubscriptionTier = subscriptionTier;
    }
    
    public int MaxAccountsPerPlatformBySubscription()
    {
        return GetRuleForEachPlan<int>(1, 3, 10);
    }

    //Return total storage to SubscriptionTier plan in megabytes
    public long TotalStorageAllowed()
    {
        return GetRuleForEachPlan<long>(500, 5000, 50000);
    }

    public TimeSpan TotalTimeToStorageRetention()
    {
        return GetRuleForEachPlan<TimeSpan>(TimeSpan.FromDays(180), TimeSpan.FromDays(730), TimeSpan.FromDays(3000));
    }

    private T GetRuleForEachPlan<T>(T free, T pro, T business)
    {
        return SubscriptionTier switch
        {
            SubscriptionEnum.FREE => free,
            SubscriptionEnum.PRO => pro,
            SubscriptionEnum.BUSINESS => business,
            _ => throw new DomainException($"Subscription {SubscriptionTier} is not supported.")
        };
    }
}