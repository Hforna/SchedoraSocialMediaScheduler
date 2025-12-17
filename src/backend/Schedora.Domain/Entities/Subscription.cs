using Schedora.Domain.Exceptions;

namespace Schedora.Domain.Entities;

public class Subscription : IEntity
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string GatewayProvider { get; set; } = string.Empty;
    public string GatewaySubscriptionId { get; set; } = string.Empty;
    public string GatewayCustomerId { get; set; } = string.Empty;
    public string GatewayPriceId { get; set; } = string.Empty;
    public SubscriptionStatus Status { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CanceledAt { get; set; }
}

public static class SubscriptionRules
{
    public static int MaxAccountsPerPlatformBySubscription(SubscriptionEnum subscription)
    {
        return subscription switch
        {
            SubscriptionEnum.FREE => 1,
            SubscriptionEnum.PRO => 3,
            SubscriptionEnum.BUSINESS => 10,
            _ => throw new DomainException($"Subscription {subscription} is not supported.")
        };
    }
}