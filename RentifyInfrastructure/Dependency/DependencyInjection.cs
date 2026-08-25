using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Responses;
using RentifyApplication.IServices;
using RentifyInfrastructure.Metrics;
using RentifyInfrastructure.Services;

namespace RentifyInfrastructure.Dependency;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException(
                "OpenAI API key is not configured.");

        services.AddSingleton(new ResponsesClient(apiKey));

        services.AddScoped<ISearchIntentService, SearchIntentService>();

        services.AddSingleton<LlmMetrics>();

        return services;
    }
}