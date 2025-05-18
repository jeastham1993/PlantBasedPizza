using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantBasedPizza.Kitchen.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeAdapter_kitchen_KitchenRequestOrderIdentifier",
                table: "RecipeAdapter");

            migrationBuilder.RenameColumn(
                name: "KitchenRequestOrderIdentifier",
                table: "RecipeAdapter",
                newName: "KitchenRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_RecipeAdapter_KitchenRequestOrderIdentifier",
                table: "RecipeAdapter",
                newName: "IX_RecipeAdapter_KitchenRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeAdapter_kitchen_KitchenRequestId",
                table: "RecipeAdapter",
                column: "KitchenRequestId",
                principalTable: "kitchen",
                principalColumn: "OrderIdentifier",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeAdapter_kitchen_KitchenRequestId",
                table: "RecipeAdapter");

            migrationBuilder.RenameColumn(
                name: "KitchenRequestId",
                table: "RecipeAdapter",
                newName: "KitchenRequestOrderIdentifier");

            migrationBuilder.RenameIndex(
                name: "IX_RecipeAdapter_KitchenRequestId",
                table: "RecipeAdapter",
                newName: "IX_RecipeAdapter_KitchenRequestOrderIdentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeAdapter_kitchen_KitchenRequestOrderIdentifier",
                table: "RecipeAdapter",
                column: "KitchenRequestOrderIdentifier",
                principalTable: "kitchen",
                principalColumn: "OrderIdentifier");
        }
    }
}
