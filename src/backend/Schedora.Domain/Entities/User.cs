using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Schedora.Domain.Exceptions;

namespace Schedora.Domain.Entities;

public class User : IdentityUser<long>, IEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool IsActive { get; set; } = true;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpires { get; set; }
    public SubscriptionEnum SubscriptionTier { get; set; } = SubscriptionEnum.FREE; 
    public DateTime? SubscriptionExpiresAt { get; set; }
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; } = DateTime.UtcNow;
    public string? StripeCustomerId { get; }

    public void UpdatePassword(string password, string hash)
    {
        if (password.Length < 8)
            throw new DomainException("Password length must be greather than 7");

        if (!password.Any(d => d.ToString().ToUpper() == d.ToString()))
            throw new DomainException("Password must contains at least one upper letter");

        PasswordHash = hash;
    }
}

public class Role : IdentityRole<long>
{
    
}