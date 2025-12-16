using Schedora.Domain.Enums;
using Schedora.Domain.Exceptions;
using Schedora.Domain.Services;
using Stripe;

namespace Schedora.Infrastructure.Services.Payment;

public class StripePricesService : IGatewayPricesService
{
    public const string Pro = "price_1SchwV01ThTiNs0ZXZtb0aJN";
    public const string Business = "price_1Schzo01ThTiNs0ZZ9rHQYDL";

    public string ConvertSubscriptionEnumToPrice(SubscriptionEnum subscription)
    {
        return subscription switch
        {
            SubscriptionEnum.PRO => Pro,
            SubscriptionEnum.BUSINESS => Business,
            _ => throw new DomainException("Invalid subscription type")
        };
    }

    public async Task<decimal> GetPriceBySubscription(SubscriptionEnum subscription)
    {
        var priceId = ConvertSubscriptionEnumToPrice(subscription);
        
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