using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantBasedPizza.Recipes.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ingredients_recipes_RecipeIdentifier",
                table: "Ingredients");

            migrationBuilder.AddForeignKey(
                name: "FK_Ingredients_recipes_RecipeIdentifier",
                table: "Ingredients",
                column: "RecipeIdentifier",
                principalTable: "recipes",
                principalColumn: "RecipeIdentifier",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ingredients_recipes_RecipeIdentifier",
                table: "Ingredients");

            migrationBuilder.AddForeignKey(
                name: "FK_Ingredients_recipes_RecipeIdentifier",
                table: "Ingredients",
                column: "RecipeIdentifier",
                principalTable: "recipes",
                principalColumn: "RecipeIdentifier");
        }
    }
}
