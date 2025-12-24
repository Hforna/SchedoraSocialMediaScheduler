namespace Schedora.Infrastructure.EmailTemplates.Models;

public class SubscriptionActivatedModel
{
    public string SubscriptionTier  { get; set; }
    public string UserName { get; set; }
    public string CompanyName { get; set; }
}