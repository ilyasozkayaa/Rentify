using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentifyDomain.Entities;

namespace RentifyInfrastructure.Persistence.Configurations;

public sealed class RentableProductConfiguration : IEntityTypeConfiguration<RentableProduct>
{
    public void Configure(EntityTypeBuilder<RentableProduct> builder)
    {
        builder.ToTable("rentable_products");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        builder.Property(x => x.OwnerUserId).IsRequired();
        builder.Property(x => x.RentalType).IsRequired();
        builder.Property(x => x.CityCode).IsRequired();
        builder.Property(x => x.District).HasMaxLength(100);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Price).IsRequired().HasPrecision(18, 2);
        builder.Property(x => x.Currency).IsRequired().HasMaxLength(3).HasDefaultValue("TRY");
        builder.Property(x => x.Attributes).HasColumnType("jsonb");
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasOne(x => x.Owner).WithMany(x => x.RentableProducts).HasForeignKey(x => x.OwnerUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new
        {
            x.CityCode,
            x.RentalType,
            x.IsActive
        });
        builder.HasIndex(x => new
        {
            x.CityCode,
            x.District,
            x.RentalType,
            x.IsActive
        });
        builder.HasIndex(x => new
        {
            x.RentalType,
            x.Price,
            x.IsActive
        });
    }
}
