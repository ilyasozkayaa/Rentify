using MediatR;
using RentifyApplication.IRepositories;
using RentifyApplication.IServices;
using RentifyDomain.Entities;

namespace RentifyApplication.Command.CreateImageUploadBatch;

public sealed class CreateImageUploadBatchCommandHandler : IRequestHandler<CreateImageUploadBatchCommand, CreateImageUploadBatchResponse>
{
    private static readonly TimeSpan UploadLifetime = TimeSpan.FromMinutes(15);
    private readonly IRentableProductImageRepository _images;
    private readonly IImageStorage _storage;

    public CreateImageUploadBatchCommandHandler(IRentableProductImageRepository images, IImageStorage storage)
    {
        _images = images;
        _storage = storage;
    }

    public async Task<CreateImageUploadBatchResponse> Handle(CreateImageUploadBatchCommand command, CancellationToken cancellationToken)
    {
        var batchId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.Add(UploadLifetime);
        var uploads = new List<ImageUploadUrl>(command.Files.Count);
        for (var i = 0; i < command.Files.Count; i++)
        {
            var file = command.Files[i];
            var uploadId = Guid.NewGuid();
            var extension = file.ContentType.ToLowerInvariant() switch { "image/jpeg" => "jpg", "image/png" => "png", _ => "webp" };
            var storageKey = $"temporary/{command.OwnerUserId}/{batchId:N}/{uploadId:N}.{extension}";
            var presigned = await _storage.CreatePresignedUploadAsync(storageKey, file.ContentType.ToLowerInvariant(), cancellationToken);
            await _images.AddAsync(new RentableProductImage
            {
                UploadId = uploadId,
                UploadBatchId = batchId,
                OwnerUserId = command.OwnerUserId,
                StorageKey = storageKey,
                ContentType = file.ContentType.ToLowerInvariant(),
                FileSize = file.FileSize,
                SortOrder = i,
                IsPrimary = i == 0,
                ExpiresAt = expiresAt
            }, cancellationToken);
            uploads.Add(new ImageUploadUrl(uploadId, presigned.Url, storageKey, presigned.ExpiresAt));
        }

        return new CreateImageUploadBatchResponse(batchId, uploads);
    }
}
