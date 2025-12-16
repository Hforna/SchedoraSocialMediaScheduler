using Schedora.Application.Requests;
using Schedora.Domain.Enums;
using Swashbuckle.AspNetCore.Filters;

namespace Schedora.WebApi.RequestExamples;

public class CreateSubscriptionCheckoutRequestExample : IExamplesProvider<CreateSubscriptionCheckoutRequest>
{
    public CreateSubscriptionCheckoutRequest GetExamples()
    {
        return new CreateSubscriptionCheckoutRequest()
        {
            CancelUrl = "https://frontend.application.com/subscriptions/checkout/cancel",
            Subscription = SubscriptionEnum.BUSINESS,
            SuccessUrl = "https://frontend.application.com/subscriptions/checkout/success",
        };
    }
}