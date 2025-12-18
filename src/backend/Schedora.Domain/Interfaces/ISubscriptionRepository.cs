namespace Schedora.Domain.Interfaces;

public interface ISubscriptionRepository
{
    public Task<Subscription?> GetUserSubscription(long userId);
}