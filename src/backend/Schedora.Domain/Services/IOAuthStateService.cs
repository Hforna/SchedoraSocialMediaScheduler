using Schedora.Domain.Dtos;

namespace Schedora.Domain.Services;

public interface IOAuthStateService
{
    public Task StorageState(string state, long userId, string platform, string redirectUrl);
    public Task<StateResponseDto?> GetStateStoraged(string platform, string state);
}