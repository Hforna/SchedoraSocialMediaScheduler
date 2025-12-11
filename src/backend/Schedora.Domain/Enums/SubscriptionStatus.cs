namespace Schedora.Domain.Enums;

public enum SubscriptionStatus
{
    Incomplete,
    IncompleteExpired,
    Trialing,
    Active,
    PastDue,
    Canceled,
    Unpaid,
    Paused
}