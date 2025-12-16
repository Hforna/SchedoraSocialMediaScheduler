using System.ComponentModel;
using System.Reflection;
using Schedora.Domain.Exceptions;

namespace Schedora.Domain.Enums;

public enum SubscriptionEnum
{
    [Description("1 account per platform, 10 scheduled posts, 30 analytics days, 5gb storage")]
    FREE,
    [Description("3 accounts per platform, unlimited posts, 1 year of analytics, 5 gb storage")]
    [StripePrice("price_1SchwV01ThTiNs0ZXZtb0aJN")]
    PRO,
    [Description("10 accounts, crew resources, unlimited analytics, 50 gb storage")]
    [StripePrice("price_1Schzo01ThTiNs0ZZ9rHQYDL")]
    BUSINESS,
}

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());

        var attribute = field?
            .GetCustomAttribute<DescriptionAttribute>();

        return attribute?.Description ?? value.ToString();
    }
}

[AttributeUsage(AttributeTargets.Field)]
public sealed class StripePriceAttribute : Attribute
{
    public string PriceId { get; }

    public StripePriceAttribute(string priceId)
    {
        PriceId = priceId;
    }
}

public static class SubscriptionEnumExtensions
{
    public static string GetPrice(this SubscriptionEnum subscription)
    {
        var field = subscription.GetType().GetField(subscription.ToString());
        var attribute = field?.GetCustomAttributes(typeof(StripePriceAttribute), false)
            .FirstOrDefault() as StripePriceAttribute;

        if (attribute is null)
            throw new DomainException("Stripe price not exists");

        return attribute.PriceId;
    }
}