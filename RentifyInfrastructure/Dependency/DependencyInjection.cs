using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
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
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException("Redis connection string is not configured.");

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));

        services.AddDbContext<RentifyDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("PostgreConnection")));

        services.AddScoped<IRentableProductRepository, RentableProductRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRentRepository, RentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRedisCacheService, RedisCacheService>();

        return services;
    }
}