namespace Schedora.Domain.Services;

public interface ICurrentUserService
{
    public Task<User?> GetUser();
    public string GetCurrentUserTimeZone();
}