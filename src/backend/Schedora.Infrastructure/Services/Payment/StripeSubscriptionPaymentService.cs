using Microsoft.Extensions.Logging;
using Schedora.Domain.Dtos;
using Schedora.Domain.Enums;
using Schedora.Domain.Exceptions;
using Schedora.Domain.Services;
using Stripe;
using Stripe.Checkout;
using Stripe.Tax;

namespace Schedora.Infrastructure.Services.Payment;

public class StripeSubscriptionPaymentService : ISubscriptionPaymentService
{
    private readonly ILogger<StripeSubscriptionPaymentService> _logger;

    public StripeSubscriptionPaymentService(ILogger<StripeSubscriptionPaymentService> logger)
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

    public async Task CancelCurrentSubscription(string subscriptionGatewayId)
    {
         var service = new SubscriptionService();

         try
         {
             var cancel = await service.CancelAsync(subscriptionGatewayId);
         }
         catch (Exception e)
         {
             _logger.LogError(e, $"It has been occured an error while trying to cancel subscription: {subscriptionGatewayId}, {e.Message}");
             
             throw new ExternalServiceException("It was not possible to cancel user current subscription");
         }
        
    }

    public async Task<SubscriptionPaymentGatewayDto> GetSubscriptionDetails(string subscriptionId)
    {
        var service = new SubscriptionService();
        
        var result = await service.GetAsync(subscriptionId);
        var item = result.Items.First();

        return new SubscriptionPaymentGatewayDto()
        {
            CurrentPeriodEndsAt = item.CurrentPeriodEnd,
            CurrentPeriodStartsAt = item.CurrentPeriodStart,
            Id = result.Id,
            Status = result.Status,
            PriceId = item.Price.Id
        };
    }
}