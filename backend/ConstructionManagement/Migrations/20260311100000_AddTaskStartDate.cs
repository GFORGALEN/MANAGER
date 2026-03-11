using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConstructionManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskStartDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "TaskItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE TaskItems
                SET StartDate = CreatedAt
                WHERE StartDate IS NULL;
                """);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "TaskItems",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "TaskItems");
        }
    }
}
