using Schedora.Domain.Exceptions;

namespace Schedora.Domain.Services;

public interface IGatewayPricesService
{
    public string ConvertSubscriptionEnumToPrice(SubscriptionEnum subscription);
    public Task<decimal> GetPriceBySubscription(SubscriptionEnum subscription);
    public SubscriptionEnum ConvertPriceToSubscriptionEnum(string price);
}