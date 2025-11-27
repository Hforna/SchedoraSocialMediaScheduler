global using Schedora.Infrastructure.Persistence;

namespace Schedora.Infrastructure.Repositories;

public abstract class BaseRepository
{
    protected readonly DataContext _context;

    protected BaseRepository(DataContext context)
    {
        _context = context;
    }
}