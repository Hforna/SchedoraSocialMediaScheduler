using Microsoft.EntityFrameworkCore;
using Schedora.Domain.Entities;

namespace Schedora.Infrastructure.Repositories;

public class MediaRepository : BaseRepository, IMediaRepository
{
    public MediaRepository(DataContext context) : base(context)
    {
    }

    public async Task<long> GetTotalUserMediaStoraged(long userId)
    {
        return await _context.Media
            .Where(d => d.UserId == userId)
            .LongCountAsync(d => d.FileSize != 0);
    }

    public async Task<Media?> GetFirstUserMedia(long userId)
    {
        return await _context.Media.FirstOrDefaultAsync(d => d.UserId == userId);
    }

    public async Task<List<Media>> GetMediasByIds(List<long> mediaIds, long userId)
    {
        return await _context.Media.Where(d => mediaIds.Contains(d.Id) && d.UserId == userId).ToListAsync();
    }
}