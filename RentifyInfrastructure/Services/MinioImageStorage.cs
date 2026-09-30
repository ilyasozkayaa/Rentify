using Minio;
using Minio.DataModel.Args;
using Microsoft.Extensions.Configuration;
using RentifyApplication.IServices;
using RentifyApplication.Constants;

namespace RentifyInfrastructure.Services;

public sealed class MinioImageStorage : IImageStorage
{
    private static readonly HashSet<string> SupportedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private readonly IMinioClient _client;
    private readonly string _bucket;

    public MinioImageStorage(IMinioClient client, IConfiguration configuration)
    {
        _client = client;
        _bucket = configuration["ObjectStorage:Bucket"] ?? throw new InvalidOperationException("ObjectStorage bucket is not configured.");
    }

    public async Task<PresignedImageUpload> CreatePresignedUploadAsync(string storageKey, string contentType, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        if (storageKey.Contains("..", StringComparison.Ordinal) || storageKey.StartsWith("/", StringComparison.Ordinal))
            throw new ArgumentException("The storage key is invalid.", nameof(storageKey));

        if (!SupportedContentTypes.Contains(contentType))
            throw new ArgumentException("The content type is not supported.", nameof(contentType));

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
            var stat = await _client.StatObjectAsync(new StatObjectArgs().WithBucket(_bucket).WithObject(storageKey),cancellationToken);

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

        return _client.PresignedGetObjectAsync(new PresignedGetObjectArgs().WithBucket(_bucket).WithObject(storageKey).WithExpiry(ProjectConstants.DownloadExpirySeconds));
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

        await _client.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(_bucket).WithObject(temporaryStorageKey),cancellationToken);
    }

    public async Task<bool> ValidateImageContentAsync(string storageKey, string contentType, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        var buffer = new byte[12];
        var totalRead = 0;

        await _client.GetObjectAsync(new GetObjectArgs().WithBucket(_bucket).WithObject(storageKey).WithCallbackStream(stream =>
                {
                    while (totalRead < buffer.Length)
                    {
                        var read = stream.Read(buffer, totalRead, buffer.Length - totalRead);

                        if (read == 0)
                            break;

                        totalRead += read;
                    }
                }),
            cancellationToken);

        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => totalRead >= 3 &&
                            buffer[0] == 0xFF &&
                            buffer[1] == 0xD8 &&
                            buffer[2] == 0xFF,

            "image/png" => totalRead >= 8 &&
                           buffer[0] == 0x89 &&
                           buffer[1] == 0x50 &&
                           buffer[2] == 0x4E &&
                           buffer[3] == 0x47 &&
                           buffer[4] == 0x0D &&
                           buffer[5] == 0x0A &&
                           buffer[6] == 0x1A &&
                           buffer[7] == 0x0A,

            "image/webp" => totalRead >= 12 &&
                            buffer[0] == 0x52 &&
                            buffer[1] == 0x49 &&
                            buffer[2] == 0x46 &&
                            buffer[3] == 0x46 &&
                            buffer[8] == 0x57 &&
                            buffer[9] == 0x45 &&
                            buffer[10] == 0x42 &&
                            buffer[11] == 0x50,

            _ => false
        };
    }
}