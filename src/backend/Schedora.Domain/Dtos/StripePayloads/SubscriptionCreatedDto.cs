namespace Schedora.Domain.Dtos.StripePayloads;

public class SubscriptionCreatedDto
{
    public string Id { get; set; }
    public string Customer { get; set; }
    public string Status { get; set; }
    public long CurrentPeriodStart { get; set; }
    public long CurrentPeriodEnd { get; set; }
    public List<SubscriptionCreatedDataDto> Items { get; set; }
}

public class SubscriptionCreatedDataDto
{
    public SubscriptionCreatedPriceDto Price { get; set; }
}

public class SubscriptionCreatedPriceDto
{
    public string Id { get; set; }
    public decimal UnitAmount  { get; set; }
    public string Currency { get; set; }
}