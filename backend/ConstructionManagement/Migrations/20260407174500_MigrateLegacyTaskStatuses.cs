using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConstructionManagement.Migrations
{
    public partial class MigrateLegacyTaskStatuses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE TaskItems
                SET Status = CASE
                    WHEN Status = 'Todo' THEN 'Draft'
                    WHEN Status = 'Doing' THEN 'InProgress'
                    ELSE Status
                END
                WHERE Status IN ('Todo', 'Doing', 'Done', 'Draft', 'InProgress', 'Blocked');
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE TaskItems
                SET Status = CASE
                    WHEN Status = 'Draft' THEN 'Todo'
                    WHEN Status = 'InProgress' THEN 'Doing'
                    ELSE Status
                END
                WHERE Status IN ('Todo', 'Doing', 'Done', 'Draft', 'InProgress', 'Blocked');
                """);
        }
    }
}
