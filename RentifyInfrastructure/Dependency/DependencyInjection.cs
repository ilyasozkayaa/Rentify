using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Responses;
using RentifyApplication.IServices;
using RentifyInfrastructure.Services;

namespace RentifyInfrastructure.Dependency;

#pragma warning disable OPENAI001
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException(
                "OpenAI API key is not configured.");

        services.AddSingleton(
            new ResponsesClient(apiKey));

        services.AddScoped<ISearchIntentService, SearchIntentService>();

        return services;
    }
}
#pragma warning restore OPENAI001