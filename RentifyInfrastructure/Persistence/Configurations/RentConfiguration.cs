using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentifyDomain.Entities;

namespace RentifyInfrastructure.Persistence.Configurations;

public sealed class RentConfiguration : IEntityTypeConfiguration<Rent>
{
    public void Configure(EntityTypeBuilder<Rent> builder)
    {
        builder.ToTable("rents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
        builder.Property(x => x.RenterUserId).IsRequired();
        builder.Property(x => x.RentableProductId).IsRequired();
        builder.Property(x => x.StartDate).IsRequired();
        builder.Property(x => x.EndDate).IsRequired();
        builder.Property(x => x.TotalPrice).IsRequired().HasPrecision(18, 2);
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasOne(x => x.RenterUser).WithMany(x => x.Rents).HasForeignKey(x => x.RenterUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.RentableProduct).WithMany(x => x.Rents).HasForeignKey(x => x.RentableProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new
        {
            x.RenterUserId,
            x.Status
        });
        builder.HasIndex(x => new
        {
            x.RentableProductId,
            x.StartDate,
            x.EndDate
        });
    }
}
