namespace Schedora.Domain.Interfaces;

public interface IUserRepository
{
    public Task<bool> UserByEmailExists(string email);
    public Task<User?> UserByEmail(string email);
    public Task<User?> UserByEmailNotConfirmed(string email);
    public Task<List<User>> GetUsersNotActiveByEmail(string email);
}