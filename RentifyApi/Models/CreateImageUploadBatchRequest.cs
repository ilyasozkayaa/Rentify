namespace RentifyApi.Models;

public sealed record CreateImageUploadBatchRequest(IReadOnlyList<ImageFileMetadataRequest> Files);
public sealed record ImageFileMetadataRequest(string ContentType, long FileSize);
