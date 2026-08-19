using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrangepuffPortal.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCultureCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "sCultureCode",
                schema: "identity",
                table: "Users",
                type: "varchar(10)",
                nullable: false,
                defaultValue: "en-US");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sCultureCode",
                schema: "identity",
                table: "Users");
        }
    }
}
