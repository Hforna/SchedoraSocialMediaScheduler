namespace Schedora.Domain.Interfaces;

public interface IStorageRepository
{
    public Task<long> GetTotalStorageUsedByUser(long userId);
}