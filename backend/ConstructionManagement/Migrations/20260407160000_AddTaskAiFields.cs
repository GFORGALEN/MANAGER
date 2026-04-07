using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConstructionManagement.Migrations
{
    public partial class AddTaskAiFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "TaskItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedHours",
                table: "TaskItems",
                type: "decimal(6,1)",
                precision: 6,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "TaskItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Medium");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "EstimatedHours",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "TaskItems");
        }
    }
}
