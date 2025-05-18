using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantBasedPizza.Deliver.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AddressId",
                table: "DeliveryRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRequests_AddressId",
                table: "DeliveryRequests",
                column: "AddressId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryRequests_Address_AddressId",
                table: "DeliveryRequests",
                column: "AddressId",
                principalTable: "Address",
                principalColumn: "AddressId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryRequests_Address_AddressId",
                table: "DeliveryRequests");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryRequests_AddressId",
                table: "DeliveryRequests");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "DeliveryRequests");
        }
    }
}
