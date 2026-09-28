using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentifyDomain.Entities;

namespace RentifyInfrastructure.Persistence.Configurations;

public sealed class RentableProductImageConfiguration : IEntityTypeConfiguration<RentableProductImage>
{
    public void Configure(EntityTypeBuilder<RentableProductImage> builder)
    {
        builder.ToTable("rentable_product_images");
        builder.HasKey(x => x.UploadId);
        builder.Property(x => x.StorageKey).IsRequired().HasMaxLength(512);
        builder.Property(x => x.ContentType).IsRequired().HasMaxLength(64);
        builder.Property(x => x.FileSize).IsRequired();
        builder.Property(x => x.SortOrder).IsRequired();
        builder.Property(x => x.IsPrimary).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.HasOne(x => x.Owner).WithMany().HasForeignKey(x => x.OwnerUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.RentableProduct).WithMany(x => x.Images).HasForeignKey(x => x.RentableProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.OwnerUserId, x.UploadBatchId, x.RentableProductId });
        builder.HasIndex(x => new { x.RentableProductId, x.SortOrder });
        builder.HasIndex(x => x.ExpiresAt).HasFilter("\"RentableProductId\" IS NULL");
        builder.HasIndex(x => x.RentableProductId).IsUnique().HasFilter("\"IsPrimary\" = TRUE AND \"RentableProductId\" IS NOT NULL");
    }
}
