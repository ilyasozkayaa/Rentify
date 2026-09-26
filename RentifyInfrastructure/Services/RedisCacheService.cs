using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RentifyApplication.IServices;
using StackExchange.Redis;

namespace RentifyInfrastructure.Services;

public sealed class RedisCacheService : IRedisCacheService
{
    private readonly IConnectionMultiplexer _connection;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly TimeSpan _defaultTtl;

    public RedisCacheService(
        IConnectionMultiplexer connection,
        IConfiguration configuration,
        ILogger<RedisCacheService> logger)
    {
        _connection = connection;
        _logger = logger;
        _defaultTtl = TimeSpan.FromSeconds(Math.Max(1, configuration.GetValue("RedisCache:DefaultTtlSeconds", 30)));
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _connection.GetDatabase().KeyExistsAsync(key);
        }
        catch (RedisException exception)
        {
            _logger.LogWarning(exception, "Unable to check Redis cache key {CacheKey}", key);
            return false;
        }
    }

    public async Task SetAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        try
        {
            await _connection.GetDatabase().StringSetAsync(key, value, _defaultTtl);
        }
        catch (RedisException exception)
        {
            _logger.LogWarning(exception, "Unable to set Redis cache key {CacheKey}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _connection.GetDatabase().KeyDeleteAsync(key);
        }
        catch (RedisException exception)
        {
            _logger.LogWarning(exception, "Unable to remove Redis cache key {CacheKey}; it will expire by TTL", key);
        }
    }
}