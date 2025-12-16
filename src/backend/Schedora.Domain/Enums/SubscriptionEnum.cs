using System.ComponentModel;
using System.Reflection;

namespace Schedora.Domain.Enums;

public enum SubscriptionEnum
{
    [Description("1 account per platform, 10 scheduled posts, 30 analytics days, 5gb storage")]
    FREE,
    [Description("3 accounts per platform, unlimited posts, 1 year of analytics, 5 gb storage")]
    PRO,
    [Description("10 accounts, crew resources, unlimited analytics, 50 gb storage")]
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