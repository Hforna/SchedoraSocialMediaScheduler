namespace Schedora.Domain.Services;

public interface IOAuthStateService
{
    public Task StorageState(string state, long userId, string platform);
    public Task<string> GetStateStoraged(string platform, long userId);
}