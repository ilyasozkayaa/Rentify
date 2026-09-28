using RentifyApplication.Command;

namespace RentifyApplication.Command.CreateImageUploadBatch;

public sealed record ImageUploadFile(string ContentType, long FileSize);
public sealed record CreateImageUploadBatchCommand(int OwnerUserId, IReadOnlyList<ImageUploadFile> Files) : ICommand<CreateImageUploadBatchResponse>;
public sealed record ImageUploadUrl(Guid UploadId, string Url, string StorageKey, DateTime ExpiresAt);
public sealed record CreateImageUploadBatchResponse(Guid UploadBatchId, IReadOnlyList<ImageUploadUrl> Uploads);
