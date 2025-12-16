namespace Schedora.Application.Responses;

public class SubscriptionPlansResponse
{
    public List<SubscriptionPlanResponse>  Plans { get; set; }
}

public class SubscriptionPlanResponse
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
}