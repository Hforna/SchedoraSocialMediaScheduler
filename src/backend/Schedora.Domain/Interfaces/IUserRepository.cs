namespace Schedora.Domain.Interfaces;

public interface IUserRepository
{
    public Task<bool> UserByEmailExists(string email);
}