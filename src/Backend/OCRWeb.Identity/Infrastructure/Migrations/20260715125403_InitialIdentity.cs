using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCRWeb.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    sUsername = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    sEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    sDisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    sPasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    btIsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    dtInsertedTime = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    dtUpdatedTime = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_Users_Username",
                schema: "identity",
                table: "Users",
                column: "sUsername",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users",
                schema: "identity");
        }
    }
}
