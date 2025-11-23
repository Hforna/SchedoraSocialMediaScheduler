using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Schedora.Infrastructure.Persistence;

namespace Schedora.Infrastructure;

public static class ServicesConfiguration
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddDbContext(services, configuration);
    }

    static void AddDbContext(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DataContext>(d => d.UseSqlServer(configuration.GetConnectionString("sqlserver")));
    }
}