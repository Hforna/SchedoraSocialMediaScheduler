namespace Schedora.Domain.Interfaces;

public interface IUnitOfWork
{
    public IGenericRepository GenericRepository { get; set; }
    public IUserRepository UserRepository { get; set; }
    public ISocialAccountRepository SocialAccountRepository { get; set; }
    public IMediaRepository MediaRepository { get; set; }
    public Task Commit();
}