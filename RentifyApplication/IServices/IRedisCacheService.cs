namespace RentifyApplication.IServices;

public interface IRedisCacheService
{
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    Task SetAsync(string key, string value, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}