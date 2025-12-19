global using Schedora.Domain.Interfaces;
global using Schedora.Infrastructure.Persistence;

namespace Schedora.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly DataContext _context;

    public UnitOfWork(DataContext context, IGenericRepository genericRepository, 
        IUserRepository userRepository,  ISocialAccountRepository socialAccountRepository,  
        IMediaRepository mediaRepository, ISubscriptionRepository subscriptionRepository)
    {
        _context = context;
        
        SubscriptionRepository = subscriptionRepository;
        SocialAccountRepository = socialAccountRepository;
        MediaRepository = mediaRepository;
        GenericRepository = genericRepository;
        UserRepository = userRepository;
    }

    public IGenericRepository GenericRepository { get; set; }
    public IUserRepository UserRepository { get; set; }
    public ISocialAccountRepository SocialAccountRepository { get; set; }
    public IMediaRepository MediaRepository { get; set; }
    public ISubscriptionRepository SubscriptionRepository { get; set; }


    public async Task Commit()
    {
        await _context.SaveChangesAsync();
    }
}