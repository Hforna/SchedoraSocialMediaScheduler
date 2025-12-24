using Schedora.Domain.DomainServices;
using Schedora.Domain.Dtos;
using SocialScheduler.Domain.Constants;

namespace Schedora.Application.Services;

public interface ISubscriptionService
{
    public Task<string> CreateSubscriptionCheckout(CreateSubscriptionCheckoutRequest request);
    public Task<SubscriptionPlansResponse> GetSubscriptionPlans();
    public Task<UserSubscriptionPlanResponse> GetCurrentUserSubscriptionPlan();
    public Task<UsageLimitsResponse> GetUsageLimits();
    public Task CancelCurrentSubscription();
}

public class SubscriptionService : ISubscriptionService
{
    public SubscriptionService(ILogger<SubscriptionService> logger, ISubscriptionPaymentService subscriptionPaymentService, 
        ICustomerPaymentService customerPaymentService, ITokenService tokenService, 
        IMapper mapper, IGatewayPricesService  gatewayPrices, 
        IActivityLogService activityLogService, IUnitOfWork uow, ISocialAccountDomainService socialAccountDomainService,  IMediaDomainService mediaDomainService)
    {
        _logger = logger;
        _activityLogService = activityLogService;
        _subscriptionPaymentService = subscriptionPaymentService;
        _customerPaymentService = customerPaymentService;
        _socialAccountDomainService = socialAccountDomainService;
        _mediaDomainService = mediaDomainService;
        _gatewayPrices = gatewayPrices;
        _uow = uow;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    private readonly ILogger<SubscriptionService> _logger;
    private readonly ISubscriptionPaymentService  _subscriptionPaymentService;
    private readonly ICustomerPaymentService  _customerPaymentService;
    private readonly ISocialAccountDomainService _socialAccountDomainService;
    private readonly IMediaDomainService _mediaDomainService;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly IGatewayPricesService _gatewayPrices;
    private readonly IActivityLogService _activityLogService;
    private readonly IUnitOfWork _uow;
    
    public async Task<string> CreateSubscriptionCheckout(CreateSubscriptionCheckoutRequest request)
    {
        var user = await _tokenService.GetUserByToken();

        var customerGatewayId = user.GatewayCustomerId;
        
        if (string.IsNullOrEmpty(user.GatewayCustomerId))
        {
            if (user.Address is null)
                throw new DomainException("User must update address information before make checkout");

            try
            {
                var userAddress = _mapper.Map<UserAddressDto>(user.Address);

                customerGatewayId = await _customerPaymentService.CreateCustomer(
                    user.Id,
                    $"{user.FirstName} {user.LastName}",
                    user.Email,
                    user.PhoneNumber,
                    userAddress);
                
                user.GatewayCustomerId = customerGatewayId;
                _uow.GenericRepository.Update<User>(user);
                await _uow.Commit();
            }
            catch (Exception e)
            {
                _logger.LogError(e,  e.Message);
                
                throw new InternalServiceException("It has occured an error while trying to create payment checkout");
            }
        }
        
        var result = await _subscriptionPaymentService.CreateCheckoutSession(customerGatewayId, request.Subscription, 
            request.SuccessUrl, request.CancelUrl);
        
        return result;
    }

    public async Task<SubscriptionPlansResponse> GetSubscriptionPlans()
    {
        var plans = Enum.GetValues<SubscriptionEnum>().ToList();
        var priceTasks = plans.Select(async plan =>
        {
            decimal price = 0;
            if(plan != SubscriptionEnum.FREE)
                price = await _gatewayPrices.GetPriceBySubscription(plan);

            return new
            {
                Plan = plan,
                Price = price
            };
        });

        var prices = await Task.WhenAll(priceTasks);
        
        var response = new SubscriptionPlansResponse()
        {
            Plans = prices.Select(d =>
            {
                var planResponse = new SubscriptionPlanResponse()
                {
                    Name = d.Plan.ToString(),
                    Price = d.Price,
                    Description = d.Plan.GetDescription()
                };

                return planResponse;
            }).ToList()
        };

        return response;
    }
    
    public async Task<UserSubscriptionPlanResponse> GetCurrentUserSubscriptionPlan()
    {
        var user = await _tokenService.GetUserByToken();

        var response = new UserSubscriptionPlanResponse()
        {
            Name = user.Subscription.SubscriptionTier.ToString(),
            Description = user.Subscription.SubscriptionTier.GetDescription()
        };

        if (user.Subscription.SubscriptionTier == SubscriptionEnum.FREE)
            return response;

        response.Price = await _gatewayPrices.GetPriceBySubscription(user.Subscription.SubscriptionTier);
        response.ExpiresAt = user.Subscription.CurrentPeriodEndsAt;
        
        return response;
    }

    public async Task<UsageLimitsResponse> GetUsageLimits()
    {
        var user = await _tokenService.GetUserByToken();
        
        var accountsCount = await _uow.SocialAccountRepository.GetUserSocialAccountConnectedPerPlatform(user!.Id);
        var accountLimit = user.Subscription.MaxAccountsPerPlatformBySubscription();
        
        var platformLimitsResponse = accountsCount.Select((platform) =>
        {
            var response = new UsageLimitsConnectedAccountsResponse()
            {
                Platform = platform.Key,
                SocialAccountsConnected = platform.Value,
                SocialAccountsLimit = accountLimit
            };

            return response;
        }).ToList();

        var totalStoraged = await _uow.MediaRepository.GetTotalUserMediaStoraged(user.Id);
        var storageLimit = user.Subscription.TotalStorageAllowed();

        var response = new UsageLimitsResponse()
        {
            ConnectedAccounts = platformLimitsResponse,
            TotalStorageMb = totalStoraged,
            LimitStorageMb = storageLimit,
            MediaRetentionEndsAt = await _mediaDomainService.GetTimeToMediaRetentEnds(user.Id, user.Subscription),
        };
        
        return response;
    }

    public async Task CancelCurrentSubscription()
    {
        var user = await _tokenService.GetUserByToken();

        if (user.Subscription.SubscriptionTier == SubscriptionEnum.FREE)
            throw new UnauthorizedException("User cannot cancel their subscription because it's a free tier");

        await _subscriptionPaymentService.CancelCurrentSubscription(user.GatewayCustomerId!);
    }
}