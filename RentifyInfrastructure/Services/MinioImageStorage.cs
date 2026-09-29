using Minio;
using Minio.DataModel.Args;
using Microsoft.Extensions.Configuration;
using RentifyApplication.IServices;
using RentifyApplication.Constants;

namespace RentifyInfrastructure.Services;

public sealed class MinioImageStorage : IImageStorage
{
    private readonly IMinioClient _client;
    private readonly string _bucket;

    public MinioImageStorage(IMinioClient client, IConfiguration configuration)
    {
        _client = client;
        _bucket = configuration["ObjectStorage:Bucket"]
            ?? throw new InvalidOperationException("ObjectStorage bucket is not configured.");
    }

    public async Task<PresignedImageUpload> CreatePresignedUploadAsync(string storageKey, string contentType, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        var url = await _client.PresignedPutObjectAsync(new PresignedPutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(storageKey)
            .WithHeaders(new Dictionary<string, string> { ["Content-Type"] = contentType })
            .WithExpiry(ProjectConstants.UploadExpirySeconds));

        return new PresignedImageUpload(url, DateTime.UtcNow.AddSeconds(ProjectConstants.UploadExpirySeconds));
    }

    public async Task<bool> ObjectExistsAsync(string storageKey, long expectedSize, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageKey);

        if (expectedSize <= 0)
            return false;

        try
        {
            var stat = await _client.StatObjectAsync(
                new StatObjectArgs()
                    .WithBucket(_bucket)
                    .WithObject(storageKey),
                cancellationToken);

            return stat.Size == expectedSize;
        }
        catch (Minio.Exceptions.ObjectNotFoundException)
        {
            return false;
        }
    }

    public Task<string> CreatePresignedDownloadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageKey);

        return _client.PresignedGetObjectAsync(new PresignedGetObjectArgs()
            .WithBucket(_bucket)
            .WithObject(storageKey)
            .WithExpiry(ProjectConstants.DownloadExpirySeconds));
    }

    public async Task PromoteAsync(string temporaryStorageKey, string permanentStorageKey, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(temporaryStorageKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(permanentStorageKey);

        await _client.CopyObjectAsync(
            new CopyObjectArgs()
                .WithBucket(_bucket)
                .WithObject(permanentStorageKey)
                .WithCopyObjectSource(
                    new CopySourceObjectArgs()
                        .WithBucket(_bucket)
                        .WithObject(temporaryStorageKey)),
            cancellationToken);

        await _client.RemoveObjectAsync(
            new RemoveObjectArgs()
                .WithBucket(_bucket)
                .WithObject(temporaryStorageKey),
            cancellationToken);
    }
}