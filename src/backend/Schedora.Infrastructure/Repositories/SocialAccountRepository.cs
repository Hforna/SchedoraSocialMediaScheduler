using Microsoft.EntityFrameworkCore;
using Schedora.Domain.Entities;

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

    public async Task<List<SocialAccount>> GetUserSocialAccounts(long userId, string platform)
    {
        return await _context.SocialAccounts
            .Where(d => d.UserId == userId && d.Platform == platform).ToListAsync();
    }
}