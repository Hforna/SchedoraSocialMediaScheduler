global using Schedora.Domain.Interfaces;
global using Schedora.Infrastructure.Persistence;
using Microsoft.Identity.Client.Extensions.Msal;

namespace Schedora.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly DataContext _context;

    public UnitOfWork(DataContext context, IGenericRepository genericRepository, 
        IUserRepository userRepository,  ISocialAccountRepository socialAccountRepository,  
        IMediaRepository mediaRepository, ISubscriptionRepository subscriptionRepository, 
        ITeamMemberRepository  teamMemberRepository, IStorageRepository storageRepository)
    {
        _context = context;
        
        StorageRepository = storageRepository;
        SubscriptionRepository = subscriptionRepository;
        TeamMemberRepository = teamMemberRepository;
        SocialAccountRepository = socialAccountRepository;
        MediaRepository = mediaRepository;
        GenericRepository = genericRepository;
        UserRepository = userRepository;
    }

    public IGenericRepository GenericRepository { get; set; }
    public IUserRepository UserRepository { get; set; }
    public IStorageRepository StorageRepository { get; set; }
    public ISocialAccountRepository SocialAccountRepository { get; set; }
    public ITeamMemberRepository TeamMemberRepository { get; set; }
    public IMediaRepository MediaRepository { get; set; }
    public ISubscriptionRepository SubscriptionRepository { get; set; }


    public async Task Commit()
    {
        await _context.SaveChangesAsync();
    }
}