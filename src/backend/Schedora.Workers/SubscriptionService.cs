using Microsoft.Extensions.Logging;
using Schedora.Domain.Entities;
using Schedora.Domain.Enums;
using Schedora.Domain.Interfaces;

namespace Schedora.Workers;

public interface ISubscriptionService
{
    Task HandleSubscriptionsCancellations();
}

public class SubscriptionService : ISubscriptionService
{
    private readonly IUnitOfWork _uow;
    private ILogger<SubscriptionService> _logger;
    private readonly IServiceProvider  _serviceProvider;

    public SubscriptionService(IUnitOfWork uow, ILogger<SubscriptionService> logger, IServiceProvider serviceProvider)
    {
        _uow = uow;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task HandleSubscriptionsCancellations()
    {
        var usersCanceled = await _uow.UserRepository.GetUsersWithCanceledSubscription();

        if (!usersCanceled.Any())
            return;

        var usersExpired = usersCanceled.Where(d => DateTime.UtcNow >= d.Subscription.CurrentPeriodEndsAt).ToList();

        foreach (var user in usersExpired)
        {
            user.Subscription.Status = SubscriptionStatus.Active;
            user.Subscription.SubscriptionTier = SubscriptionEnum.FREE;

            var socialAccounts = await _uow.SocialAccountRepository.GetAllUserSocialAccounts(user.Id);
            socialAccounts = socialAccounts.Select(d =>
            {
                d.IsActive = false;

                return d;
            }).ToList();
            
            _uow.GenericRepository.UpdateRange<SocialAccount>(socialAccounts);
        }
        
        _uow.GenericRepository.UpdateRange<User>(usersExpired);
        await _uow.Commit();
    }
}