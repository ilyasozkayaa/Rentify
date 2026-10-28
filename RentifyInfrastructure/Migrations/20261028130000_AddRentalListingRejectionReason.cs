using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RentifyInfrastructure.Persistence;

#nullable disable

namespace RentifyInfrastructure.Migrations;

[DbContext(typeof(RentifyDbContext))]
[Migration("20261028130000_AddRentalListingRejectionReason")]
public partial class AddRentalListingRejectionReason : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "RejectionReason",
            table: "rentable_products",
            type: "character varying(1000)",
            maxLength: 1000,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "RejectionReason", table: "rentable_products");
    }
}
