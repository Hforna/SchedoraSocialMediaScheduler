using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Schedora.Domain.Entities;
using Schedora.Domain.Interfaces;
using Schedora.Domain.RabbitMq.Producers;
using Schedora.Domain.Services;
using Schedora.Infrastructure.ExternalServices;
using Schedora.Infrastructure.Persistence;
using Schedora.Infrastructure.RabbitMq;
using Schedora.Infrastructure.RabbitMq.Producers;
using Schedora.Infrastructure.Repositories;
using Schedora.Infrastructure.Services;

namespace Schedora.Infrastructure;

public static class ServicesConfiguration
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddDbContext(services, configuration);
        AddServices(services, configuration);
        AddRepositories(services);
        AddProducers(services, configuration);
        AddConsumers(services);
    }

    static void AddDbContext(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DataContext>(d => d.UseSqlServer(configuration.GetConnectionString("sqlserver")));
        
        services.AddIdentity<User, Role>()
            .AddEntityFrameworkStores<DataContext>()
            .AddDefaultTokenProviders()
            .AddUserManager<UserManager<User>>();
    }

    static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITokenService, TokenService>(d =>
        {
            using var scope = d.CreateScope();

            return new TokenService(
                scope.ServiceProvider.GetRequiredService<ILogger<ITokenService>>(),
                scope.ServiceProvider.GetRequiredService<IRequestService>(),
                scope.ServiceProvider.GetRequiredService<IServiceProvider>(),
                scope.ServiceProvider.GetRequiredService<TokenValidationParameters>(),
                configuration.GetValue<string>("services:auth:jwt:signKey")!,
                configuration.GetValue<int>("services:auth:jwt:expiresAt"));
        });
        services.AddSingleton<ICryptographyService, CryptographyService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IRequestService, RequestService>();
        services.AddScoped<IActivityLogService, ActivityLogService>();
        services.AddScoped<IExternalOAuthAuthenticationService, TwitterExternalOAuthAuthenticationService>();
        services.AddScoped<IExternalOAuthAuthenticationService, LinkedInOAuthAuthenticationService>();
        services.AddScoped<ILinkedInService, LinkedInService>();
    }

    static void AddProducers(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ISocialAccountProducer, SocialAccountProducer>();
    }

    static void AddConsumers(IServiceCollection services)
    {
        
    }

    static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IGenericRepository, GenericRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
    }
}