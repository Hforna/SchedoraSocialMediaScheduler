namespace Schedora.Domain.Services;

public interface IOAuthStateService
{
    public Task StorageState(string state, long userId, string platform);
    public Task<long?> GetUserIdByStateStoraged(string platform, string state);
}