using Schedora.Domain.Enums;
using Schedora.Domain.Exceptions;

namespace Schedora.Infrastructure.Services.Payment;

public class StripeSubscriptionPrices
{
    public const string Pro = "price_1SchwV01ThTiNs0ZXZtb0aJN";
    public const string Business = "price_1Schzo01ThTiNs0ZZ9rHQYDL";

    public static string ConvertSubscriptionEnumToPrice(SubscriptionEnum subscription)
    {
        return subscription switch
        {
            SubscriptionEnum.PRO => Pro,
            SubscriptionEnum.BUSINESS => Business,
            _ => throw new DomainException("Invalid subscription type to update")
        };
    }
}