using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Schedora.Application.Services;

namespace Schedora.Application;

public static class ServicesConfiguration
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        AddServices(services);
        AddMapper(services);
    }

    static void AddServices(IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ISocialAccountService, SocialAccountService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IStripeWebhookService, StripeWebhookService>();
    }

    static void AddMapper(IServiceCollection services)
    {
        services.AddAutoMapper(d => d.AddProfile<ConfigurationMapperService>());
    }
}