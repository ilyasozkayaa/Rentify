namespace RentifyApplication.IServices;

public sealed record PresignedImageUpload(string Url, DateTime ExpiresAt);

public interface IImageStorage
{
    Task<PresignedImageUpload> CreatePresignedUploadAsync(string storageKey, string contentType, CancellationToken cancellationToken = default);
    Task<string> CreatePresignedDownloadAsync(string storageKey, CancellationToken cancellationToken = default);
    Task<bool> ObjectExistsAsync(string storageKey, long expectedSize, CancellationToken cancellationToken = default);
    Task PromoteAsync(string temporaryStorageKey, string permanentStorageKey, CancellationToken cancellationToken = default);
}
