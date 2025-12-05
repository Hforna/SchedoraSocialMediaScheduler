using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Schedora.Domain.Dtos;
using Schedora.Domain.Services.Cache;

namespace Schedora.Infrastructure.Services.Cache;

public class SocialAccountCache : ISocialAccountCache
{
    private readonly IDistributedCache _cache;

    public SocialAccountCache(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task AddStateAuthorization(string state, long userId, string platform)
    {
        var dto = new SocialAccountStateDto() { State =  state};
        var serialize = JsonSerializer.Serialize(dto);
        var bytes = Encoding.UTF8.GetBytes(serialize);
        await _cache.SetAsync($"state:{platform}:{userId}", bytes, new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(60)));
    }

    public async Task<string> GetStateAuthorization(long userId, string platform)
    {
        var stateBytes = await _cache.GetAsync($"state:{platform}:{userId}");

        if (stateBytes is null)
            return "";
        
        var state =  Encoding.UTF8.GetString(stateBytes);
        var deserialize = JsonSerializer.Deserialize<SocialAccountStateDto>(state);
        
        return deserialize.State;
    }
}