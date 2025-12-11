using Schedora.Domain.Enums;
using Schedora.Domain.Exceptions;
using Schedora.Domain.Services;

namespace Schedora.Infrastructure.Services.Payment;

public class StripeSubscriptionPrices : IGatewayPricesService
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
    
    public SubscriptionEnum ConvertPriceToSubscriptionEnum(string price)
    {
        return price switch
        {
            Pro => SubscriptionEnum.PRO,
            Business => SubscriptionEnum.BUSINESS,
            _ => throw new DomainException("Invalid price")
        };
    }
}