namespace Schedora.Domain.Interfaces;

public interface IGenericRepository
{
    public Task<T?> GetById<T>(long id) where T : class, IEntity;
    public Task Add<T>(T entity) where T : class, IEntity;
    public void Update<T>(T entity) where T : class, IEntity;
    public void UpdateRange<T>(List<T> entity) where T : class, IEntity;
    public void DeleteRange<T>(List<T> entities) where T : class, IEntity;
}