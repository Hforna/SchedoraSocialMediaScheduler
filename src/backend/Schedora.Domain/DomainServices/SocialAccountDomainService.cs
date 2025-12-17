using Microsoft.Extensions.Logging;
using Schedora.Domain.Exceptions;
using Schedora.Domain.Interfaces;

namespace Schedora.Domain.DomainServices;

public interface ISocialAccountDomainService
{
    public Task<bool> UserAbleToConnectAccount(User user, SubscriptionEnum userSubscriptionPlan, string platform);
}

public class SocialAccountDomainService : ISocialAccountDomainService
{
    public SocialAccountDomainService(IUnitOfWork uow, ILogger<SocialAccountDomainService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    private readonly IUnitOfWork _uow;
    private readonly ILogger<SocialAccountDomainService> _logger;
    
    public async Task<bool> UserAbleToConnectAccount(User user, SubscriptionEnum userSubscriptionPlan, string platform)
    {
        var userSocialAccounts = await _uow.SocialAccountRepository.GetUserSocialAccounts(user.Id, platform);

        if (!userSocialAccounts.Any())
            return true;

        var subscrptionType = user.SubscriptionTier;
        
        var userReached =  UserReachedMaxAccounts(userSocialAccounts.Count + 1, subscrptionType);
        var accountsCanConnect = SubscriptionRules.MaxAccountsPerPlatformBySubscription(subscrptionType);
        
        if(userReached)
            throw new DomainException($"User only can connect to  {accountsCanConnect} accounts");

        return true;
    }

    private bool UserReachedMaxAccounts(int totalConnected, SubscriptionEnum subscription)
    {
        return SubscriptionRules.MaxAccountsPerPlatformBySubscription(subscription) <= totalConnected;
    }
}