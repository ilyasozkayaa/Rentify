using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RentifyInfrastructure.Persistence;

#nullable disable

namespace RentifyInfrastructure.Migrations;

[DbContext(typeof(RentifyDbContext))]
[Migration("20260928120000_AddRentableProductImages")]
partial class AddRentableProductImages
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        RentifyDbContextModelSnapshot.BuildCurrentModel(modelBuilder);
    }
}
