using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCRWeb.ProjectManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameProjectPkToIId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "project",
                table: "Projects",
                newName: "iId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "iId",
                schema: "project",
                table: "Projects",
                newName: "Id");
        }
    }
}
