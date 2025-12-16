using Schedora.Domain.Enums;
using Schedora.Domain.Exceptions;
using Schedora.Domain.Services;
using Stripe;

namespace Schedora.Infrastructure.Services.Payment;

public class StripePricesService : IGatewayPricesService
{

    public async Task<decimal> GetPriceBySubscription(SubscriptionEnum subscription)
    {
        var priceId = subscription.GetPrice();
        
        var service = new PriceService();
        var price = await service.GetAsync(priceId);

        if (price is null)
            throw new InternalServiceException("It was not possible to get subscription plan price");

        return (decimal)(price.UnitAmount / 100)!;
    }

    public SubscriptionEnum ConvertPriceToSubscriptionEnum(string price)
    {
        return Enum.TryParse<SubscriptionEnum>(price, true, out SubscriptionEnum priceEnum) 
            ? priceEnum 
            : throw new InternalServiceException("Invalid subscription type");
    }
}