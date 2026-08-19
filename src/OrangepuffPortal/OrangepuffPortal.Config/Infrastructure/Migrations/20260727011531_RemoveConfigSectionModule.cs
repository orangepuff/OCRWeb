using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrangepuffPortal.Config.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveConfigSectionModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_ConfigSections_Module_TextCode",
                schema: "config",
                table: "ConfigSections");

            migrationBuilder.DropColumn(
                name: "sModule",
                schema: "config",
                table: "ConfigSections");

            migrationBuilder.CreateIndex(
                name: "UQ_ConfigSections_TextCode",
                schema: "config",
                table: "ConfigSections",
                column: "sTextCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_ConfigSections_TextCode",
                schema: "config",
                table: "ConfigSections");

            migrationBuilder.AddColumn<string>(
                name: "sModule",
                schema: "config",
                table: "ConfigSections",
                type: "varchar(60)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "UQ_ConfigSections_Module_TextCode",
                schema: "config",
                table: "ConfigSections",
                columns: new[] { "sModule", "sTextCode" },
                unique: true);
        }
    }
}
