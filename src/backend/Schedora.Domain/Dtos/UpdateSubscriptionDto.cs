namespace Schedora.Domain.Dtos;

public class UpdateSubscriptionDto
{
    public string CustomerId { get; set; }
    public string PriceId { get; set; }
    public string SubscriptionId { get; set; }
    public string Status  { get; set; }
}