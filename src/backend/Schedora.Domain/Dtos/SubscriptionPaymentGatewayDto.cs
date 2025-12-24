namespace Schedora.Domain.Dtos;

public class SubscriptionPaymentGatewayDto
{
    public string Id { get; set; }
    public string PriceId { get; set; }
    public DateTime CurrentPeriodEndsAt  { get; set; }
    public DateTime CurrentPeriodStartsAt  { get; set; }
    public string Status { get; set; }
}