using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrangepuffPortal.Config.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigDefaultValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "btDefaultValue",
                schema: "config",
                table: "Configs",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "iDefaultValue",
                schema: "config",
                table: "Configs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "nDefaultValue",
                schema: "config",
                table: "Configs",
                type: "decimal(8,3)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sDefaultValue",
                schema: "config",
                table: "Configs",
                type: "nvarchar(255)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "btDefaultValue",
                schema: "config",
                table: "Configs");

            migrationBuilder.DropColumn(
                name: "iDefaultValue",
                schema: "config",
                table: "Configs");

            migrationBuilder.DropColumn(
                name: "nDefaultValue",
                schema: "config",
                table: "Configs");

            migrationBuilder.DropColumn(
                name: "sDefaultValue",
                schema: "config",
                table: "Configs");
        }
    }
}
