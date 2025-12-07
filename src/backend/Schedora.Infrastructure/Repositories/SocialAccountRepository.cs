using Microsoft.EntityFrameworkCore;

namespace Schedora.Infrastructure.Repositories;

public class SocialAccountRepository : BaseRepository, ISocialAccountRepository
{
    public SocialAccountRepository(DataContext context) : base(context)
    {
    }

    public async Task<bool> SocialAccountLinkedToUserExists(long userId, string platformUserId, string platform)
    {
        return await _context.SocialAccounts
            .AnyAsync(d => d.UserId == userId && d.PlatformUserId == platformUserId && d.Platform == platform);
    }
}