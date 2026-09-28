namespace RentifyDomain.Entities;

public sealed class RentableProductImage
{
    public Guid UploadId { get; set; }
    public Guid UploadBatchId { get; set; }
    public int OwnerUserId { get; set; }
    public int? RentableProductId { get; set; }
    public string StorageKey { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long FileSize { get; set; }
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public RentableProduct? RentableProduct { get; set; }
    public User Owner { get; set; } = null!;
}
