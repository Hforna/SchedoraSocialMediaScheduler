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
        
        var subscriptionType = _gatewayPricesService.ConvertPriceToSubscriptionEnum(priceId);
        var subscriptionStatus = SubscriptionStatus.Incomplete;
        
        switch (subscriptionDto.Status)
        {
            case SubscriptionStatuses.Incomplete:
                subscriptionStatus = SubscriptionStatus.Incomplete;
                break;
            case SubscriptionStatuses.Active:
                subscriptionStatus = SubscriptionStatus.Active;
                break;
            case SubscriptionStatuses.Canceled:
                subscriptionStatus = SubscriptionStatus.Canceled;
                break;
            case SubscriptionStatuses.Unpaid:
                subscriptionStatus = SubscriptionStatus.Unpaid;
                break;
            case SubscriptionStatuses.IncompleteExpired:
                subscriptionStatus = SubscriptionStatus.IncompleteExpired;
                break;
            case SubscriptionStatuses.Trialing:
                subscriptionStatus = SubscriptionStatus.Trialing;
                break;
            case SubscriptionStatuses.PastDue:
                subscriptionStatus = SubscriptionStatus.PastDue;
                break;
            case SubscriptionStatuses.Paused:
                subscriptionStatus = SubscriptionStatus.Paused;
                break;
        }
        
        var subscription = new Subscription
        (
            user.Id,
            subscriptionType,
            "stripe",
            subscriptionStatus,
            customerId,
            priceId,
            subscriptionDto.Id,
            DateTime.UtcNow.AddSeconds(subscriptionDto.CurrentPeriodEnd),
            DateTime.UtcNow.AddSeconds(subscriptionDto.CurrentPeriodStart)
        );
        
        await _uow.GenericRepository.Add<Subscription>(subscription);

        await _uow.Commit();
    }
}