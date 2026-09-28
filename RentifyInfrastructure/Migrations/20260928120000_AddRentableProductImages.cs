using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentifyInfrastructure.Migrations;

public partial class AddRentableProductImages : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "rentable_product_images",
            columns: table => new
            {
                UploadId = table.Column<Guid>(type: "uuid", nullable: false),
                UploadBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                OwnerUserId = table.Column<int>(type: "integer", nullable: false),
                RentableProductId = table.Column<int>(type: "integer", nullable: true),
                StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                ContentType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                FileSize = table.Column<long>(type: "bigint", nullable: false),
                SortOrder = table.Column<int>(type: "integer", nullable: false),
                IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_rentable_product_images", x => x.UploadId);
                table.ForeignKey("FK_rentable_product_images_rentable_products_RentableProductId", x => x.RentableProductId, "rentable_products", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_rentable_product_images_users_OwnerUserId", x => x.OwnerUserId, "users", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_rentable_product_images_OwnerUserId_UploadBatchId_RentableProductId", "rentable_product_images", new[] { "OwnerUserId", "UploadBatchId", "RentableProductId" });
        migrationBuilder.CreateIndex("IX_rentable_product_images_RentableProductId_SortOrder", "rentable_product_images", new[] { "RentableProductId", "SortOrder" });
        migrationBuilder.CreateIndex("IX_rentable_product_images_ExpiresAt", "rentable_product_images", "ExpiresAt", filter: "\"RentableProductId\" IS NULL");
        migrationBuilder.CreateIndex("IX_rentable_product_images_RentableProductId", "rentable_product_images", "RentableProductId", unique: true, filter: "\"IsPrimary\" = TRUE AND \"RentableProductId\" IS NOT NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("rentable_product_images");
    }
}
