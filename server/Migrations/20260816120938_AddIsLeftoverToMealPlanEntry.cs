using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PantryPlan.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddIsLeftoverToMealPlanEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLeftover",
                table: "MealPlanEntries",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLeftover",
                table: "MealPlanEntries");
        }
    }
}
