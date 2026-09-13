using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PantryPlan.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerIdToIngredient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Ingredients",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_OwnerId",
                table: "Ingredients",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ingredients_AspNetUsers_OwnerId",
                table: "Ingredients",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ingredients_AspNetUsers_OwnerId",
                table: "Ingredients");

            migrationBuilder.DropIndex(
                name: "IX_Ingredients_OwnerId",
                table: "Ingredients");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Ingredients");
        }
    }
}
