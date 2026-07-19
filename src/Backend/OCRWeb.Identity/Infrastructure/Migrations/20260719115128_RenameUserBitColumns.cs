using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCRWeb.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserBitColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "btIsTemplateUser",
                schema: "identity",
                table: "Users",
                newName: "btTemplateUser");

            migrationBuilder.RenameColumn(
                name: "btIsActive",
                schema: "identity",
                table: "Users",
                newName: "btActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "btTemplateUser",
                schema: "identity",
                table: "Users",
                newName: "btIsTemplateUser");

            migrationBuilder.RenameColumn(
                name: "btActive",
                schema: "identity",
                table: "Users",
                newName: "btIsActive");
        }
    }
}
