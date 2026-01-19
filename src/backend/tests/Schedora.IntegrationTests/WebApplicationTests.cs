using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Schedora.Domain.Entities;
using Schedora.Domain.Enums;
using Schedora.Domain.Interfaces;
using Schedora.Domain.Services;
using Schedora.Infrastructure.Persistence;
using Schedora.Workers;
using Testcontainers.MsSql;

namespace Schedora.IntegrationTests;

public class WebApplicationTests : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2019-latest")
        .Build();
    
    public DataContext? DbContext { get; set; }
    public User User { get; set; }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        var connectionString = _sqlContainer.GetConnectionString();

        builder.ConfigureAppConfiguration(d =>
        {
            d.Sources.Clear();

            d.AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Test.json")
                .AddEnvironmentVariables();
        });
        
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<DataContext>>();
            
            services.RemoveAll<IBackgroundJobClient>();
            services.RemoveAll<IRecurringJobManager>();
            services.RemoveAll<JobStorage>();
            services.RemoveAll<IWorkerScheduler>();
            
            services.AddDbContext<DataContext>(options => options.UseSqlServer(connectionString));
        });
    }

    public async Task SeedDataTest()
    {
        using var scope = Services.CreateScope();
        var provider = scope.ServiceProvider;
        
        var uow =  provider.GetRequiredService<IUnitOfWork>();
        var passwordHasher = provider.GetRequiredService<IPasswordCryptographyService>();

        var user = new User()
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
        {
            await DbContext.DisposeAsync();
        }
        await _sqlContainer.DisposeAsync();
    }
}