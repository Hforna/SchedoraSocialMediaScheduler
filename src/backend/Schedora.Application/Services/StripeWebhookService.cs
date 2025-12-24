using System.Text.Json;
using Stripe;
using Subscription = Schedora.Domain.Entities.Subscription;

namespace Schedora.Application.Services;

public interface IStripeWebhookService
{
    public Task HandleInvoiceSubscriptionEvent(Invoice invoice);
}

public class StripeWebhookService : IStripeWebhookService
{
    public StripeWebhookService(IUnitOfWork uow, IGatewayPricesService gatewayPricesService, 
        ILogger<StripeWebhookService> logger, IEmailService emailService, ISubscriptionPaymentService subscriptionPaymentService)
    {
        _uow = uow;
        _gatewayPricesService = gatewayPricesService;
        _logger = logger;
        _emailService = emailService;
        _subscriptionPaymentService = subscriptionPaymentService;
    }

    private readonly IUnitOfWork _uow;
    private readonly IGatewayPricesService  _gatewayPricesService;
    private readonly ILogger<StripeWebhookService> _logger;
    private readonly IEmailService _emailService;
    private readonly ISubscriptionPaymentService _subscriptionPaymentService;

    public async Task HandleInvoiceSubscriptionEvent(Invoice invoice)
    {
        var customerId = invoice.CustomerId;
        
        var user = await _uow.UserRepository.GetUserByCustomerGatewayId(customerId)
            ?? throw new NotFoundException("User by customer stripe api was not found");

        var lineData = invoice.Lines.First();
        
        var priceId = lineData.Pricing.PriceDetails.Price;
        _logger.LogInformation("Gateway price id");
        
        var subscriptionType = _gatewayPricesService.ConvertPriceToSubscriptionEnum(priceId);

        var subscriptionGateway = await _subscriptionPaymentService.GetSubscriptionDetails(lineData.SubscriptionId);
        
        var subscription = user.Subscription;
        subscription!.GatewayPriceId = priceId;
        subscription.CurrentPeriodEndsAt = subscriptionGateway.CurrentPeriodEndsAt;
        subscription.CreatedAt = subscriptionGateway.CurrentPeriodStartsAt;
        subscription.GatewaySubscriptionId = subscriptionGateway.Id;
        subscription.GatewayProvider = "stripe";
        subscription.SubscriptionTier = subscriptionType;
        subscription.Status = Enum.Parse<SubscriptionStatus>(subscriptionGateway.Status, true);

        var emailTemplate = await _emailService.RenderSubscriptionActivated(subscription.SubscriptionTier.ToString(),
            user.UserName, CompanyConstraints.CompanyName);

        await _emailService.SendEmail(user.Email, user.UserName, emailTemplate, $"Hi {user.FirstName}  {user.LastName} your subscription has been updated");
        
        _uow.GenericRepository.Update<Subscription>(subscription);
        await _uow.Commit();
    }
}