using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Schedora.Domain.Entities;

public class User : IdentityUser<long>
{
    public bool IsActive { get; set; } = true;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpires { get; set; }
    public SubscriptionEnum SubscriptionTier { get; set; } = SubscriptionEnum.FREE; 
    public DateTime? SubscriptionExpiresAt { get; set; }
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; } = DateTime.UtcNow;
    public string? StripeCustomerId { get; }
}

public class Role : IdentityRole<long>
{
    public Role(string role) : base(role) {}
}