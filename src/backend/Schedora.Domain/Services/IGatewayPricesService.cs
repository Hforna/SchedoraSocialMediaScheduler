using Schedora.Domain.Exceptions;

namespace Schedora.Domain.Services;

public interface IGatewayPricesService
{
    public Task<decimal> GetPriceBySubscription(SubscriptionEnum subscription);
    public SubscriptionEnum ConvertPriceToSubscriptionEnum(string price);
}