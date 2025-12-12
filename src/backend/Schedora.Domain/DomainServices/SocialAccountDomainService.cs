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

        if (subscrptionType == SubscriptionEnum.FREE)
            throw new DomainException("User already have an account connected to this platform");
        if (subscrptionType == SubscriptionEnum.PRO && userSocialAccounts.Count >= 3)
            throw new DomainException("User only can connect to 3 accounts per platform");
        if (subscrptionType == SubscriptionEnum.BUSINESS && userSocialAccounts.Count >= 10)
            throw new DomainException("User only can connect to 10 accounts");

        return true;
    }
}