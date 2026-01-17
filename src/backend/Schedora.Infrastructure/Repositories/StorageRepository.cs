using Microsoft.EntityFrameworkCore;

namespace Schedora.Infrastructure.Repositories;

public class StorageRepository : BaseRepository, IStorageRepository
{
    public StorageRepository(DataContext context) : base(context)
    {
    }

    public async Task<long> GetTotalStorageUsedByUser(long userId)
    {
        return await _context.Media
            .Where(d => d.UserId == userId)
            .SumAsync(d => d.FileSize);
    }
}