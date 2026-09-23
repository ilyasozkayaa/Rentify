using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RentifyInfrastructure.Persistence;

#nullable disable

namespace RentifyInfrastructure.Migrations;

public partial class UseRentableProductStatus : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex("IX_rentable_products_CityCode_District_RentalType_IsActive", "rentable_products");
        migrationBuilder.DropIndex("IX_rentable_products_CityCode_RentalType_IsActive", "rentable_products");
        migrationBuilder.DropIndex("IX_rentable_products_RentalType_Price_IsActive", "rentable_products");

        migrationBuilder.RenameColumn("IsActive", "rentable_products", "Status");
        migrationBuilder.Sql("""
            ALTER TABLE rentable_products ALTER COLUMN "Status" DROP DEFAULT;
            ALTER TABLE rentable_products ALTER COLUMN "Status" TYPE integer
                USING CASE WHEN "Status" THEN 1 ELSE 0 END;
            ALTER TABLE rentable_products ALTER COLUMN "Status" SET DEFAULT 1;
            """);

        migrationBuilder.CreateIndex("IX_rentable_products_CityCode_District_RentalType_Status", "rentable_products", new[] { "CityCode", "District", "RentalType", "Status" });
        migrationBuilder.CreateIndex("IX_rentable_products_CityCode_RentalType_Status", "rentable_products", new[] { "CityCode", "RentalType", "Status" });
        migrationBuilder.CreateIndex("IX_rentable_products_RentalType_Price_Status", "rentable_products", new[] { "RentalType", "Price", "Status" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex("IX_rentable_products_CityCode_District_RentalType_Status", "rentable_products");
        migrationBuilder.DropIndex("IX_rentable_products_CityCode_RentalType_Status", "rentable_products");
        migrationBuilder.DropIndex("IX_rentable_products_RentalType_Price_Status", "rentable_products");

        migrationBuilder.Sql("""
            ALTER TABLE rentable_products ALTER COLUMN "Status" DROP DEFAULT;
            ALTER TABLE rentable_products ALTER COLUMN "Status" TYPE boolean
                USING CASE WHEN "Status" = 1 THEN true ELSE false END;
            ALTER TABLE rentable_products ALTER COLUMN "Status" SET DEFAULT true;
            """);
        migrationBuilder.RenameColumn("Status", "rentable_products", "IsActive");

        migrationBuilder.CreateIndex("IX_rentable_products_CityCode_District_RentalType_IsActive", "rentable_products", new[] { "CityCode", "District", "RentalType", "IsActive" });
        migrationBuilder.CreateIndex("IX_rentable_products_CityCode_RentalType_IsActive", "rentable_products", new[] { "CityCode", "RentalType", "IsActive" });
        migrationBuilder.CreateIndex("IX_rentable_products_RentalType_Price_IsActive", "rentable_products", new[] { "RentalType", "Price", "IsActive" });
    }
}
