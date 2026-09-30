using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentifyInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxPayload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Payload",
                table: "outbox_messages",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Payload",
                table: "outbox_messages");
        }
    }
}
