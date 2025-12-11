using Schedora.Domain.Dtos;
using Schedora.Domain.Enums;
using Schedora.Domain.Services;
using Stripe;

namespace Schedora.Infrastructure.Services.Payment;

public class StripeSubscriptionService : ISubscriptionService
{
    public async Task<UpdateSubscriptionDto> UpdateSubscription(string customerGatewayId, SubscriptionEnum subscription)
    {
        var price = StripeSubscriptionPrices.ConvertSubscriptionEnumToPrice(subscription);
        
        var options = new SubscriptionCreateOptions
        {
            Customer = customerGatewayId,
            Items = new List<SubscriptionItemOptions>
            {
                new SubscriptionItemOptions
                {
                    Price = price
                }
            },
            PaymentBehavior = "default_incomplete",
            PaymentSettings = new SubscriptionPaymentSettingsOptions
            {
                PaymentMethodTypes = new List<string> { "card" }
            }
        };

        var service = new SubscriptionService();
        var result = await service.CreateAsync(options);

        var response = new UpdateSubscriptionDto()
        {
            CustomerId = result.CustomerId,
            PriceId = price,
            Status = result.Status,
            SubscriptionId = result.Id,
        };

        return response;
    }
}