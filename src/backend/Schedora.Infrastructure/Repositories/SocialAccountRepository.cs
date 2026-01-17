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

    public async Task<Dictionary<string, int>> GetUserSocialAccountConnectedPerPlatform(long userId)
    {
        return await _context.SocialAccounts
            .AsNoTracking()
            .Where(d => d.UserId == userId)
            .GroupBy(d => d.Platform)
            .ToDictionaryAsync(d => d.Key, f => f.Count());
    }

    public async Task<List<SocialAccount>> GetAllTwitterSocialAccounts()
    {
        return await _context.SocialAccounts.Where(d => d.Platform == SocialPlatformsNames.Twitter)
            .OrderBy(d => d.LastTokenRefreshAt)
            .ToListAsync();
    }

    public async Task<List<SocialAccount>> GetSocialAccountsByIds(List<long> socialAccountIds)
    {
        return await _context.SocialAccounts.Where(d => socialAccountIds.Contains(d.Id)).ToListAsync();
    }

    public async Task<List<SocialAccount>> GetAllUserSocialAccounts(long userId)
    {
        return await _context.SocialAccounts.Where(d => d.UserId == userId).ToListAsync();
    }
}