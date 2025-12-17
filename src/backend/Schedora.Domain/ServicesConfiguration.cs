using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Schedora.Domain.DomainServices;

namespace Schedora.Domain;

public static class ServicesConfiguration
{
    public static void AddDomain(this IServiceCollection services, IConfiguration configuration)
    {
        AddServices(services, configuration);
    }

    static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISocialAccountDomainService, SocialAccountDomainService>();
        services.AddScoped<ISubscriptionDomainService, SubscriptionDomainService>();
        services.AddScoped<IMediaDomainService, MediaDomainService>();
    }
}