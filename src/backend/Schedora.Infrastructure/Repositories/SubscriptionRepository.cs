using Microsoft.EntityFrameworkCore;
using Schedora.Domain.Entities;

namespace Schedora.Infrastructure.Repositories;

public class SubscriptionRepository : BaseRepository, ISubscriptionRepository
{
    public SubscriptionRepository(DataContext context) : base(context)
    {
    }

    public async Task<Subscription?> GetUserSubscription(long userId)
    {
        return await _context.Subscriptions.SingleOrDefaultAsync(d => d.UserId == userId);
    }
}