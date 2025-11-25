namespace Schedora.Domain.Interfaces;

public interface IGenericRepository
{
    public Task<T?> GetById<T>(long id) where T : class, IEntity;
    public Task Add<T>(T entity) where T : class, IEntity;
}