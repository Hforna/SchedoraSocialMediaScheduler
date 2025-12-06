using Schedora.Domain.Services;
using Schedora.Domain.Services.Cache;

namespace Schedora.Infrastructure.Services;

public class OAuthStateService : IOAuthStateService
{
    private readonly ISocialAccountCache _socialAccountCache;

    public OAuthStateService(ISocialAccountCache socialAccountCache)
    {
        _socialAccountCache = socialAccountCache;
    }

    public async Task StorageState(string state, long userId, string platform)
    {
        await _socialAccountCache.AddStateAuthorization(state, userId, platform);
    }

    public async Task<string> GetStateStoraged(string platform, long userId)
    {
        return await _socialAccountCache.GetStateAuthorization(userId, platform);
    }
}