using System.Text.Json;
using Schedora.Domain.Dtos.StripePayloads;
using Stripe;
using Subscription = Schedora.Domain.Entities.Subscription;

namespace Schedora.Application.Services;

public interface IStripeWebhookService
{
    public Task HandleSubscriptionCreatedEvent(string payload);
}

public class StripeWebhookService : IStripeWebhookService
{
    public StripeWebhookService(IUnitOfWork uow, IGatewayPricesService gatewayPricesService, ILogger<StripeWebhookService> logger, IEmailService emailService)
    {
        _uow = uow;
        _gatewayPricesService = gatewayPricesService;
        _logger = logger;
        _emailService = emailService;
    }

    private readonly IUnitOfWork _uow;
    private readonly IGatewayPricesService  _gatewayPricesService;
    private readonly ILogger<StripeWebhookService> _logger;
    private readonly IEmailService _emailService;

    public async Task HandleSubscriptionCreatedEvent(string payload)
    {
        var subscriptionDto = JsonSerializer.Deserialize<SubscriptionCreatedDto>(payload);

        if (subscriptionDto is null)
        {
            _logger.LogError("There was an error while tyring to deserialize payment gateway payload: {payload}", payload);
            throw new RequestException("It was not possible to process json payload");
        }
        
        var customerId = subscriptionDto.Customer;
        
        var user = await _uow.UserRepository.GetUserByCustomerGatewayId(customerId)
            ?? throw new NotFoundException("User by customer stripe api was not found");

        var priceId = subscriptionDto.Items.FirstOrDefault()!.Price.Id;
        _logger.LogInformation("Gateway price id");
        
        var subscription = new Subscription()
        {
            GatewayCustomerId = customerId,
            GatewayPriceId = priceId,
            UserId = user.Id,
            GatewayProvider = "stripe",
            GatewaySubscriptionId = subscriptionDto.Id,
            CurrentPeriodEnd = DateTime.UtcNow.AddSeconds(subscriptionDto.CurrentPeriodEnd),
            CreatedAt = DateTime.UtcNow.AddSeconds(subscriptionDto.CurrentPeriodStart)
        };

        switch (subscriptionDto.Status)
        {
            case SubscriptionStatuses.Incomplete:
                subscription.Status = SubscriptionStatus.Incomplete;
                break;
            case SubscriptionStatuses.Active:
                subscription.Status = SubscriptionStatus.Active;
                break;
            case SubscriptionStatuses.Canceled:
                subscription.Status = SubscriptionStatus.Canceled;
                break;
            case SubscriptionStatuses.Unpaid:
                subscription.Status = SubscriptionStatus.Unpaid;
                break;
            case SubscriptionStatuses.IncompleteExpired:
                subscription.Status = SubscriptionStatus.IncompleteExpired;
                break;
            case SubscriptionStatuses.Trialing:
                subscription.Status = SubscriptionStatus.Trialing;
                break;
            case SubscriptionStatuses.PastDue:
                subscription.Status = SubscriptionStatus.PastDue;
                break;
            case SubscriptionStatuses.Paused:
                subscription.Status = SubscriptionStatus.Paused;
                break;
        }
        
        await _uow.GenericRepository.Add<Subscription>(subscription);

        if (subscription.Status != SubscriptionStatus.Active)
        {
            var priceEnum = _gatewayPricesService.ConvertPriceToSubscriptionEnum(priceId);
            
            user.SubscriptionExpiresAt = subscription.CurrentPeriodEnd;
            user.SubscriptionTier = priceEnum;
            
            _uow.GenericRepository.Update<User>(user);
        }

        await _uow.Commit();
    }
}