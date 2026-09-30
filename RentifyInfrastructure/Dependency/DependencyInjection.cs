using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using RentifyApplication.IRepositories;
using RentifyApplication.IServices;
using RentifyInfrastructure.BackgroundServices;
using RentifyInfrastructure.Metrics;
using RentifyInfrastructure.Persistence;
using RentifyInfrastructure.Repositories;
using RentifyInfrastructure.Services;
using StackExchange.Redis;

namespace RentifyInfrastructure.Dependency;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISearchIntentService, SearchIntentService>();
        var storageEndpoint = configuration["ObjectStorage:Endpoint"] ?? throw new InvalidOperationException("ObjectStorage endpoint is not configured.");
        var storageAccessKey = configuration["ObjectStorage:AccessKey"] ?? throw new InvalidOperationException("ObjectStorage access key is not configured.");
        var storageSecretKey = configuration["ObjectStorage:SecretKey"] ?? throw new InvalidOperationException("ObjectStorage secret key is not configured.");
        services.AddSingleton<IMinioClient>(_ => new MinioClient().WithEndpoint(storageEndpoint).WithCredentials(storageAccessKey, storageSecretKey).Build());
        services.AddScoped<IImageStorage, MinioImageStorage>();

        services.AddSingleton<LlmMetrics>();
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException("Redis connection string is not configured.");

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));

        services.AddDbContext<RentifyDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("PostgreConnection")));

        services.AddScoped<IRentableProductRepository, RentableProductRepository>();
        services.AddScoped<IRentableProductImageRepository, RentableProductImageRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRentRepository, RentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRedisCacheService, RedisCacheService>();
        services.AddScoped<IIdempotencyStore, RedisIdempotencyStore>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}