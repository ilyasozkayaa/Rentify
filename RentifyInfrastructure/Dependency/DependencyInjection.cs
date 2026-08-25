using Microsoft.Extensions.DependencyInjection;
using RentifyApplication.IServices;
using RentifyInfrastructure.Metrics;
using RentifyInfrastructure.Services;

namespace RentifyInfrastructure.Dependency;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ISearchIntentService, SearchIntentService>();

        services.AddSingleton<LlmMetrics>();

        return services;
    }
}