using System.Text.Json;
using Microsoft.Extensions.Logging;
using RentifyApplication.Constants;
using RentifyApplication.IServices;
using RentifyApplication.Models.Idempotency;
using StackExchange.Redis;

namespace RentifyInfrastructure.Services;

public sealed class RedisIdempotencyStore : IIdempotencyStore
{
    private readonly IConnectionMultiplexer _connection;
    private readonly ILogger<RedisIdempotencyStore> _logger;

    public RedisIdempotencyStore(IConnectionMultiplexer connection, ILogger<RedisIdempotencyStore> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<IdempotencyResult> TryStartAsync(string key, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        try
        {
            var database = _connection.GetDatabase();

            var acquired = await database.StringSetAsync(key, ProjectConstants.ProcessingValue, ttl, When.NotExists);

            if (acquired)
                return new IdempotencyResult(true, false, null, null, null);

            var existingValue = await database.StringGetAsync(key);

            if (existingValue.IsNullOrEmpty)
                return new IdempotencyResult(false, false, null, null, null);

            if (existingValue == ProjectConstants.ProcessingValue)
                return new IdempotencyResult(false, true, null, null, null);

            var storedResponse = JsonSerializer.Deserialize<StoredIdempotencyResponse>(existingValue.ToString());

            return storedResponse is null
                ? new IdempotencyResult(false, true, null, null, null)
                : new IdempotencyResult(false, false, storedResponse.Body, storedResponse.StatusCode, storedResponse.ContentType);
        }
        catch (RedisException exception)
        {
            _logger.LogError(exception, "Failed to start idempotency operation for key {IdempotencyKey}.", key);
            throw;
        }
    }

    public async Task CompleteAsync(string key, string response, int statusCode, string? contentType, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        try
        {
            var storedResponse = new StoredIdempotencyResponse(statusCode, contentType, response);
            var value = JsonSerializer.Serialize(storedResponse);

            await _connection.GetDatabase().StringSetAsync(key, value, ttl);
        }
        catch (RedisException exception)
        {
            _logger.LogError(exception, "Failed to complete idempotency operation for key {IdempotencyKey}.", key);
            throw;
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
            _logger.LogError(exception, "Failed to remove idempotency operation for key {IdempotencyKey}.", key);
            throw;
        }
    }

    private sealed record StoredIdempotencyResponse(int StatusCode, string? ContentType, string Body);
}