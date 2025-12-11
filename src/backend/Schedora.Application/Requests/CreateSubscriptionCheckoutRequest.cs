namespace Schedora.Application.Requests;

public class CreateSubscriptionCheckoutRequest
{
    public SubscriptionEnum Subscription { get; set; }
    public string SuccessUrl  { get; set; }
    public string CancelUrl { get; set; }
}