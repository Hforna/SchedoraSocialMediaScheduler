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
        var bytes = Encoding.UTF8.GetBytes(userId.ToString());
        await _cache.SetAsync($"state:{platform}:{state}", bytes, 
            new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(200)));
    }

    public async Task<long?> GetUserIdByStateAuthorization(string platform, string state)
    {
        var stateBytes = await _cache.GetAsync($"state:{platform}:{state}");

        if (stateBytes is null)
            return null;
        
        var userId =  Encoding.UTF8.GetString(stateBytes);
        
        return long.Parse(userId);
    }

    public async Task AddCodeChallenge(string code, long userId, string platform)
    {
        var bytes = Encoding.UTF8.GetBytes(code);
        
        await _cache.SetAsync($"code_challenge:{platform}:{userId}", bytes,  
            new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(200)));
    }

    public async Task<string> GetCodeChallenge(long userId, string platform)
    {
        var codeBytes = await _cache.GetAsync($"code_challenge:{platform}:{userId}");
        
        if (codeBytes is null)
            return "";
        
        var code = Encoding.UTF8.GetString(codeBytes);
        return code;
    }
}