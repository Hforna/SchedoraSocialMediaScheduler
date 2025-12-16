using Microsoft.Extensions.Logging;
using Schedora.Domain.Dtos;
using Schedora.Domain.Enums;
using Schedora.Domain.Exceptions;
using Schedora.Domain.Services;
using Stripe;
using Stripe.Checkout;

namespace Schedora.Infrastructure.Services.Payment;

public class StripeSubscriptionPaymentService : ISubscriptionPaymentService
{
    private readonly ILogger<StripeSubscriptionPaymentService> _logger;

    public StripeSubscriptionPaymentService(ILogger<StripeSubscriptionPaymentService> logger, IGatewayPricesService pricesService)
    {
        _logger = logger;
    }
    
    public async Task<string> CreateCheckoutSession(string customerGatewayId, SubscriptionEnum subscription, string successUrl, string cancelUrl)
    {
        var price = subscription.GetPrice();

        try
        {
            var options = new SessionCreateOptions()
            {
                Customer = customerGatewayId,
                Mode = "subscription",
                PaymentMethodTypes = new List<string>() { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions()
                    {
                        Price = price,
                        Quantity = 1
                    }
                },
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return session.Url;
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            
            throw;
        }
    }
}