using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Asn1.Cms;

namespace Schedora.Workers;

public static class WorkersConfiguration
{
    public static void AddWorkers(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(d =>
        {
            d.UseSqlServerStorage(configuration.GetConnectionString("sqlserver"));
        });
        
        services.AddHangfireServer();
        
        services.AddTransient<RefreshTokenService>();
        services.AddTransient<IWorkerScheduler, WorkerScheduler>();
    }
}

public interface IWorkerScheduler
{
    public void ScheduleWorks();
}

public class WorkerScheduler : IWorkerScheduler
{
    private readonly IRecurringJobManager _recurringJobManager;

    public WorkerScheduler(IRecurringJobManager recurringJobManager)
    {
        _recurringJobManager = recurringJobManager;
    }

    public void ScheduleWorks()
    {
        RecurringJob.AddOrUpdate<RefreshTokenService>(                       
            "refresh-twitter-token-job",                                     
            d => d.RegenerateTwitterTokens(),                                
            Cron.Hourly(),                                                   
            new RecurringJobOptions()                                        
            {                                                                
                TimeZone =  TimeZoneInfo.Utc                                 
            });     
        
        RecurringJob.AddOrUpdate<SubscriptionService>(
            "user-cancel-subscription-job",
            d => d.HandleSubscriptionsCancellations(),
            Cron.Hourly(),
            new RecurringJobOptions()
            {
                TimeZone =  TimeZoneInfo.Utc
            }
        );
    }
}