using Schedora.Domain.Dtos;

namespace Schedora.Application.Services;

public interface ISubscriptionService
{
    public Task<string> CreateSubscriptionCheckout(CreateSubscriptionCheckoutRequest request);
}

public class SubscriptionService : ISubscriptionService
{
    public SubscriptionService(ILogger<SubscriptionService> logger, ISubscriptionPaymentService subscriptionPaymentService, 
        ICustomerPaymentService customerPaymentService, ITokenService tokenService, IMapper mapper)
    {
        _logger = logger;
        _subscriptionPaymentService = subscriptionPaymentService;
        _customerPaymentService = customerPaymentService;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    private readonly ILogger<SubscriptionService> _logger;
    private readonly ISubscriptionPaymentService  _subscriptionPaymentService;
    private readonly ICustomerPaymentService  _customerPaymentService;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    
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
                
                throw new InternalServiceException("It has occured an error while trying to create payment gateway customer");
            }
        }
        
        var result = await _subscriptionPaymentService.CreateCheckoutSession(customerGatewayId, request.Subscription, 
            request.SuccessUrl, request.CancelUrl);
        
        return result;
    }
}