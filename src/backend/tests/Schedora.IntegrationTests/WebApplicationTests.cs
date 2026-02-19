using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RabbitMQ.Client;
using Schedora.Domain.Entities;
using Schedora.Domain.Enums;
using Schedora.Domain.Interfaces;
using Schedora.Domain.Services;
using Schedora.Infrastructure.Persistence;
using Schedora.Workers;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;

namespace Schedora.IntegrationTests;

public class WebApplicationTests : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2019-latest")
        .Build();

    private readonly RabbitMqContainer _rabbitMqContainer = new RabbitMqBuilder("rabbitmq:3.12.11-management-alpine")
        .WithUsername("guest")
        .WithPassword("guest")
        .Build();

    public DataContext? DbContext { get; set; }

    public User? User { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration(d =>
        {
            d.Sources.Clear();

            d.AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Test.json", optional: true)
                .AddEnvironmentVariables();
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<DataContext>>();

            services.RemoveAll<IBackgroundJobClient>();
            services.RemoveAll<IRecurringJobManager>();
            services.RemoveAll<JobStorage>();
            services.RemoveAll<IWorkerScheduler>();
            services.RemoveAll<IConnection>();

            var connectionString = _sqlContainer.GetConnectionString();
            services.AddDbContext<DataContext>(options => options.UseSqlServer(connectionString));

            var rabbitMqPort = _rabbitMqContainer.GetMappedPublicPort(5672);

            services.AddSingleton<IConnection>(_ =>
            {
                var factory = new ConnectionFactory
                {
                    HostName = "localhost",
                    Port = rabbitMqPort,
                    UserName = "guest",
                    Password = "guest",
                };

                return factory.CreateConnectionAsync().GetAwaiter().GetResult();
            });
        });
    }

    public async Task SeedDataTest()
    {
        using var scope = Services.CreateScope();
        var provider = scope.ServiceProvider;

        var uow = provider.GetRequiredService<IUnitOfWork>();
        var passwordHasher = provider.GetRequiredService<IPasswordCryptographyService>();

        var user = new User
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "JohnDoe@gmail.com",
            EmailConfirmed = true,
            UserName = "JohnDoe",
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            PasswordHash = passwordHasher.HashPassword("random_cool_password")
        };

        await uow.GenericRepository.Add<User>(user);
        await uow.Commit();

        var subscription = new Subscription(user.Id, SubscriptionEnum.FREE);
        await uow.GenericRepository.Add<Subscription>(subscription);
        await uow.Commit();

        User = user;
    }

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();

        var options = new DbContextOptionsBuilder<DataContext>()
            .UseSqlServer(_sqlContainer.GetConnectionString())
            .Options;

        DbContext = new DataContext(options);

        await DbContext.Database.MigrateAsync();
        await DbContext.Database.EnsureCreatedAsync();

        await SeedDataTest();
    }

    public async Task DisposeAsync()
    {
        if (DbContext is not null)
            await DbContext.DisposeAsync();

        await _rabbitMqContainer.DisposeAsync();
        await _sqlContainer.DisposeAsync();
    }
}