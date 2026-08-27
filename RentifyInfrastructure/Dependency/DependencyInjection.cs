using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RentifyApplication.IServices;
using RentifyInfrastructure.Metrics;
using RentifyInfrastructure.Persistence;
using RentifyInfrastructure.Services;

namespace RentifyInfrastructure.Dependency;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISearchIntentService, SearchIntentService>();

        services.AddSingleton<LlmMetrics>();

        services.AddDbContext<RentifyDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("PostgreConnection")));

        return services;
    }
}