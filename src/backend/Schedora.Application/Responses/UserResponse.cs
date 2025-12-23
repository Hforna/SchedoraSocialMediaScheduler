global using Schedora.Domain.Enums;

namespace Schedora.Application.Responses;

public class UserResponse : BaseResponse
{
    public string Email { get; set; }
    public string UserName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public SubscriptionEnum SubscriptionTier { get; set; } = SubscriptionEnum.FREE; 
    public DateTime? SubscriptionExpiresAt { get; set; }
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}