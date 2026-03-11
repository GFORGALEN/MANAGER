using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConstructionManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskAssignments",
                columns: table => new
                {
                    TaskItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskAssignments", x => new { x.TaskItemId, x.UserId });
                    table.ForeignKey(
                        name: "FK_TaskAssignments_TaskItems_TaskItemId",
                        column: x => x.TaskItemId,
                        principalTable: "TaskItems",
                        principalColumn: "TaskItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskAssignments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_UserId",
                table: "TaskAssignments",
                column: "UserId");

            migrationBuilder.Sql("""
                INSERT INTO TaskAssignments (TaskItemId, UserId, AssignedAt)
                SELECT TaskItemId, AssignedUserId, SYSUTCDATETIME()
                FROM TaskItems
                WHERE AssignedUserId IS NOT NULL
                  AND NOT EXISTS (
                      SELECT 1
                      FROM TaskAssignments
                      WHERE TaskAssignments.TaskItemId = TaskItems.TaskItemId
                        AND TaskAssignments.UserId = TaskItems.AssignedUserId
                  );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskAssignments");
        }
    }
}
