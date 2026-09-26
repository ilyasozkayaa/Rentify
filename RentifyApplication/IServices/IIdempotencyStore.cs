using RentifyApplication.Models.Idempotency;

namespace RentifyApplication.IServices;

public interface IIdempotencyStore
{
    Task<IdempotencyResult> TryStartAsync(string key, TimeSpan ttl, CancellationToken cancellationToken = default);
    Task CompleteAsync(string key, string response, int statusCode, string? contentType, TimeSpan ttl, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
