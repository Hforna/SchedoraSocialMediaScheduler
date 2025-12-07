namespace Schedora.Domain.Services.Session;

public interface IUserSession
{
    public void AddUserId(long userId);
    public long GetUserId();
}