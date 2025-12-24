using Schedora.Domain.Dtos;

namespace Schedora.Domain.Services;

public interface ISubscriptionPaymentService
{
    public Task<string> CreateCheckoutSession(string customerGatewayId, SubscriptionEnum subscription, string successUrl, string cancelUrl);
    public Task CancelCurrentSubscription(string customerGatewayId);
    public Task<SubscriptionPaymentGatewayDto> GetSubscriptionDetails(string subscriptionId);
}