namespace Schedora.Domain.Services.Cache;

public interface ISocialAccountCache
{
    public Task AddStateAuthorization(string state, long userId, string platform);
    public Task<string> GetStateAuthorization(long userId, string platform);
}