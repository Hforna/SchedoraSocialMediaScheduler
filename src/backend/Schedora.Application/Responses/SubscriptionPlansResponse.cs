namespace Schedora.Application.Responses;

public class SubscriptionPlansResponse
{
    public List<SubscriptionPlanResponse>  Plans { get; set; }
}

public class SubscriptionPlanResponse
{
    public string Name { get; set; }
    public decimal Price { get; set; } = 0;
    public string Description { get; set; }
}

public class UserSubscriptionPlanResponse : SubscriptionPlanResponse
{
    public DateTime? ExpiresAt { get; set; }
}