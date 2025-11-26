using Microsoft.EntityFrameworkCore;
using Schedora.Domain.Entities;
using Schedora.Domain.Interfaces;
using Schedora.Infrastructure.Persistence;

namespace Schedora.Infrastructure.Repositories;

public class GenericRepository : BaseRepository, IGenericRepository
{
    public GenericRepository(DataContext context) : base(context)
    {
    }

    public async Task<T?> GetById<T>(long id) where T : class, IEntity
    {
        return await _context.Set<T>().SingleOrDefaultAsync(d => d.Id == id);
    }

    public async Task Add<T>(T entity) where T : class, IEntity
    {
        await _context.Set<T>().AddAsync(entity);
    }

    public void Update<T>(T entity) where T : class, IEntity
    {
        _context.Set<T>().Update(entity);
    }

    public void DeleteRange<T>(List<T> entities) where T : class, IEntity
    {
        _context.Set<T>().RemoveRange(entities);
    }
}