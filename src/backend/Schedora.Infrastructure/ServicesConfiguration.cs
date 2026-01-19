using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;
using Schedora.Domain.Entities;
using Schedora.Domain.Interfaces;
using Schedora.Domain.RabbitMq.Producers;
using Schedora.Domain.Services;
using Schedora.Domain.Services.Cache;
using Schedora.Domain.Services.Session;
using Schedora.Infrastructure.Externals.Services;
using Schedora.Infrastructure.ExternalServices;
using Schedora.Infrastructure.Persistence;
using Schedora.Infrastructure.RabbitMq;
using Schedora.Infrastructure.RabbitMq.Producers;
using Schedora.Infrastructure.Repositories;
using Schedora.Infrastructure.Services;
using Schedora.Infrastructure.Services.Cache;
using Schedora.Infrastructure.Services.Cookies;
using Schedora.Infrastructure.Services.ExternalServicesConfigs;
using Schedora.Infrastructure.Services.Payment;
using Schedora.Infrastructure.Services.Sessions;
using StackExchange.Redis;
using Stripe;
using TokenService = Schedora.Infrastructure.Services.TokenService;

namespace Schedora.Infrastructure;

public static class ServicesConfiguration
{
    public static async Task AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddDbContext(services, configuration);
        AddServices(services, configuration);
        AddRepositories(services);
        await AddRabbitMq(services, configuration);
        AddRedis(services, configuration);
    }

    static void AddDbContext(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DataContext>(d => d.UseSqlServer(configuration.GetConnectionString("sqlserver")));
        
        services.AddIdentity<User, Schedora.Domain.Entities.Role>()
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
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IRequestService, RequestService>();
        services.AddScoped<IActivityLogService, ActivityLogService>();
        services.AddScoped<IStorageService, TestClass>();
        //services.AddScoped<IStorageService, DropboxStorageService>(d => new DropboxStorageService(
            //configuration.GetValue<string>("services:storage:dropbox:apiKey"), 
            //d.CreateScope().ServiceProvider.GetRequiredService<ILogger<DropboxStorageService>>()));
        
        services.AddScoped<IExternalOAuthAuthenticationService, TwitterExternalOAuthAuthenticationService>();
        services.AddScoped<IExternalOAuthAuthenticationService, LinkedInOAuthAuthenticationService>();
        services.AddScoped<IOAuthTokenService, TwitterExternalOAuthAuthenticationService>();
        
        services.AddScoped<ILinkedInService, LinkedInService>();
        services.AddScoped<IPkceService, PkceService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IOAuthStateService,  OAuthStateService>();
        services.AddScoped<ITwitterService, TwitterService>();
        
        services.AddScoped<ITokensCryptographyService, TokensEncryptService>();
        services.AddScoped<IPasswordCryptographyService, PasswordHashService>();
        services.AddSingleton<ICryptographyService, CryptographyService>();
        
        services.AddSingleton<ITwitterOAuthConfiguration, TwitterOAuthConfiguration>();
        services.AddSingleton<ILinkedInOAuthConfiguration, LinkedInOAuthConfiguration>();
        services.AddSingleton<ITwitterConfiguration, TwitterConfiguration>();

        StripeConfiguration.ApiKey = configuration.GetValue<string>("services:payment:stripe:apiKey");
        
        services.AddScoped<ICustomerPaymentService, StripeCustomerService>();
        services.AddScoped<IGatewayPricesService, StripePricesService>();
        services.AddScoped<ISubscriptionPaymentService, StripeSubscriptionPaymentService>();
    }
    
    static void AddRedis(IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("redis");
        });

        services.AddDistributedMemoryCache();
        
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });
        
        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "keys")))
            .SetApplicationName("Schedora");
        
        services.AddScoped<IUserSession, UserSession>();
        services.AddScoped<ICookiesService, CookiesService>();

        services.AddScoped<ISocialAccountCache, SocialAccountCache>();
    }

    static async Task AddRabbitMq(IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqConnection = new RabbitMqConnection()
        {
            Host = configuration.GetValue<string>("RabbitMq:host"),
            UserName = configuration.GetValue<string>("RabbitMq:UserName"),
            Password = configuration.GetValue<string>("RabbitMq:Password"),
            Port = configuration.GetValue<int>("RabbitMq:Port"),
        };

        var connection = await new ConnectionFactory()
        {
            HostName = rabbitMqConnection.Host,
            Port = rabbitMqConnection.Port,
            UserName = rabbitMqConnection.UserName,
            Password = rabbitMqConnection.Password,
        }.CreateConnectionAsync();

        services.AddSingleton<IConnection>(connection);
        
        AddProducers(services);
        AddConsumers(services);
    }
    
    static void AddProducers(IServiceCollection services)
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
        services.AddScoped<ISocialAccountRepository, SocialAccountRepository>();
        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<ITeamMemberRepository, TeamMemberRepository>();
        services.AddScoped<IStorageRepository, StorageRepository>();
    }
}