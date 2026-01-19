using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Schedora.Domain.Entities;
using Schedora.Domain.Enums;
using Schedora.Domain.Interfaces;

namespace Schedora.Workers;

public interface ISubscriptionService
{
    public Task HandleSubscriptionsCancellations();
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

        var usersExpired = usersCanceled.Where(d => DateTime.UtcNow >= d.Subscription.CurrentPeriodEndsAt).ToList();
        
        if (!usersCanceled.Any() ||  !usersExpired.Any())
            return;

        foreach (var user in usersExpired)
        {
            user.Subscription.Status = SubscriptionStatus.Active;
            user.Subscription.SubscriptionTier = SubscriptionEnum.FREE;

            var socialAccounts = await _uow.SocialAccountRepository.GetAllUserSocialAccounts(user.Id);
            if (socialAccounts.Any())
            {
                var accountsPerPlatform = socialAccounts.GroupBy(d => d.Platform);
                foreach (var account in accountsPerPlatform)
                {
                    var platformAccounts = account.ToList();
                    var maxAccountsFree = user.Subscription.MaxAccountsPerPlatformBySubscription();
                    if (platformAccounts.Count > maxAccountsFree)
                    {
                        var notAvailableAccounts = platformAccounts
                                        .OrderByDescending(d => d.ConnectedAt)
                                        .Take(platformAccounts.Count - maxAccountsFree)
                                        .Select(d =>
                                        {
                                            d.IsActive = false;
                                            return d;
                                        }).ToList();
                        _uow.GenericRepository.UpdateRange<SocialAccount>(notAvailableAccounts);
                    }
                }
            }
        }
        
        _uow.GenericRepository.UpdateRange<User>(usersExpired);
        await _uow.Commit();
    }
}