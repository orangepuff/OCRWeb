using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrangepuffPortal.Config.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigSortOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "iSortOrder",
                schema: "config",
                table: "ConfigSections",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "iSortOrder",
                schema: "config",
                table: "Configs",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "iSortOrder",
                schema: "config",
                table: "ConfigSections");

            migrationBuilder.DropColumn(
                name: "iSortOrder",
                schema: "config",
                table: "Configs");
        }
    }
}
