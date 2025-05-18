using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PlantBasedPizza.Kitchen.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialKitchenSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "kitchen",
                columns: table => new
                {
                    OrderIdentifier = table.Column<string>(type: "text", nullable: false),
                    KitchenRequestId = table.Column<string>(type: "text", nullable: true),
                    OrderReceivedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrderState = table.Column<int>(type: "integer", nullable: false),
                    PrepCompleteOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BakeCompleteOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QualityCheckCompleteOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kitchen", x => x.OrderIdentifier);
                });

            migrationBuilder.CreateTable(
                name: "RecipeAdapter",
                columns: table => new
                {
                    RecipeAdapterId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RecipeIdentifier = table.Column<string>(type: "text", nullable: false),
                    KitchenRequestOrderIdentifier = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeAdapter", x => x.RecipeAdapterId);
                    table.ForeignKey(
                        name: "FK_RecipeAdapter_kitchen_KitchenRequestOrderIdentifier",
                        column: x => x.KitchenRequestOrderIdentifier,
                        principalTable: "kitchen",
                        principalColumn: "OrderIdentifier");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeAdapter_KitchenRequestOrderIdentifier",
                table: "RecipeAdapter",
                column: "KitchenRequestOrderIdentifier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecipeAdapter");

            migrationBuilder.DropTable(
                name: "kitchen");
        }
    }
}
