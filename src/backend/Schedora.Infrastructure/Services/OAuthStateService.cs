using Schedora.Domain.Dtos;
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

    public async Task StorageState(string state, long userId, string platform, string redirectUrl)
    {
        await _socialAccountCache.AddStateAuthorization(state, userId, platform, redirectUrl);
    }

    public async Task<StateResponseDto?> GetStateStoraged(string platform, string state)
    {
        return await _socialAccountCache.GetUserIdByStateAuthorization(platform, state);
    }
}