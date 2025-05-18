using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantBasedPizza.OrderManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_orders_OrderIdentifier",
                table: "OrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_orders_DeliveryDetails_DeliveryDetailsId",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "IX_orders_DeliveryDetailsId",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "DeliveryDetailsId",
                table: "orders");

            migrationBuilder.AddColumn<string>(
                name: "OrderIdentifier",
                table: "OrderHistory",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderIdentifier",
                table: "DeliveryDetails",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderHistory_OrderIdentifier",
                table: "OrderHistory",
                column: "OrderIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryDetails_OrderIdentifier",
                table: "DeliveryDetails",
                column: "OrderIdentifier",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryDetails_orders_OrderIdentifier",
                table: "DeliveryDetails",
                column: "OrderIdentifier",
                principalTable: "orders",
                principalColumn: "OrderIdentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderHistory_orders_OrderIdentifier",
                table: "OrderHistory",
                column: "OrderIdentifier",
                principalTable: "orders",
                principalColumn: "OrderIdentifier",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_orders_OrderIdentifier",
                table: "OrderItem",
                column: "OrderIdentifier",
                principalTable: "orders",
                principalColumn: "OrderIdentifier",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryDetails_orders_OrderIdentifier",
                table: "DeliveryDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderHistory_orders_OrderIdentifier",
                table: "OrderHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_orders_OrderIdentifier",
                table: "OrderItem");

            migrationBuilder.DropIndex(
                name: "IX_OrderHistory_OrderIdentifier",
                table: "OrderHistory");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryDetails_OrderIdentifier",
                table: "DeliveryDetails");

            migrationBuilder.DropColumn(
                name: "OrderIdentifier",
                table: "OrderHistory");

            migrationBuilder.DropColumn(
                name: "OrderIdentifier",
                table: "DeliveryDetails");

            migrationBuilder.AddColumn<int>(
                name: "DeliveryDetailsId",
                table: "orders",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_orders_DeliveryDetailsId",
                table: "orders",
                column: "DeliveryDetailsId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_orders_OrderIdentifier",
                table: "OrderItem",
                column: "OrderIdentifier",
                principalTable: "orders",
                principalColumn: "OrderIdentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_orders_DeliveryDetails_DeliveryDetailsId",
                table: "orders",
                column: "DeliveryDetailsId",
                principalTable: "DeliveryDetails",
                principalColumn: "DeliveryDetailsId");
        }
    }
}
