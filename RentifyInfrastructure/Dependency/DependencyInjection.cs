using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RentifyApplication.IRepositories;
using RentifyApplication.IServices;
using RentifyInfrastructure.Metrics;
using RentifyInfrastructure.Persistence;
using RentifyInfrastructure.Repositories;
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

        services.AddScoped<IRentableProductRepository, RentableProductRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRentRepository, RentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}