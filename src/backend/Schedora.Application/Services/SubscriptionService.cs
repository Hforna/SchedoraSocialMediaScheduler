using Schedora.Domain.Dtos;
using SocialScheduler.Domain.Constants;

namespace Schedora.Application.Services;

public interface ISubscriptionService
{
    public Task<string> CreateSubscriptionCheckout(CreateSubscriptionCheckoutRequest request);
    public Task<SubscriptionPlansResponse> GetSubscriptionPlans();
    public Task<UserSubscriptionPlanResponse> GetCurrentUserSubscriptionPlan();
    public Task<UsageLimitsResponse> GetUsageLimits();
}

public class SubscriptionService : ISubscriptionService
{
    public SubscriptionService(ILogger<SubscriptionService> logger, ISubscriptionPaymentService subscriptionPaymentService, 
        ICustomerPaymentService customerPaymentService, ITokenService tokenService, 
        IMapper mapper, IGatewayPricesService  gatewayPrices, IActivityLogService activityLogService)
    {
        _logger = logger;
        _activityLogService = activityLogService;
        _subscriptionPaymentService = subscriptionPaymentService;
        _customerPaymentService = customerPaymentService;
        _gatewayPrices = gatewayPrices;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    private readonly ILogger<SubscriptionService> _logger;
    private readonly ISubscriptionPaymentService  _subscriptionPaymentService;
    private readonly ICustomerPaymentService  _customerPaymentService;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly IGatewayPricesService _gatewayPrices;
    private readonly IActivityLogService _activityLogService;
    
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
            Name = user.SubscriptionTier.ToString(),
            Description = user.SubscriptionTier.GetDescription()
        };

        if (user.SubscriptionTier == SubscriptionEnum.FREE)
            return response;

        response.Price = await _gatewayPrices.GetPriceBySubscription(user.SubscriptionTier);
        response.ExpiresAt = user.SubscriptionExpiresAt;
        
        return response;
    }

    public async Task<UsageLimitsResponse> GetUsageLimits()
    {
        var user = await _tokenService.GetUserByToken();
        
        
    }
}