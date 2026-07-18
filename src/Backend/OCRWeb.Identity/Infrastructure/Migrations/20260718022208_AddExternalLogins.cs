using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCRWeb.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalLogins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "identity",
                table: "Users",
                newName: "iId");

            migrationBuilder.AlterColumn<string>(
                name: "sPasswordHash",
                schema: "identity",
                table: "Users",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "ExternalLogins",
                schema: "identity",
                columns: table => new
                {
                    iId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    iUserId = table.Column<int>(type: "int", nullable: false),
                    sProvider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    sProviderKey = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    dtInsertedTime = table.Column<DateTime>(type: "datetime2(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalLogins", x => x.iId);
                    table.ForeignKey(
                        name: "FK_ExternalLogins_Users_iUserId",
                        column: x => x.iUserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "iId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_ExternalLogins_Provider_ProviderKey",
                schema: "identity",
                table: "ExternalLogins",
                columns: new[] { "sProvider", "sProviderKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ExternalLogins_UserId_Provider",
                schema: "identity",
                table: "ExternalLogins",
                columns: new[] { "iUserId", "sProvider" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalLogins",
                schema: "identity");

            migrationBuilder.RenameColumn(
                name: "iId",
                schema: "identity",
                table: "Users",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "sPasswordHash",
                schema: "identity",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);
        }
    }
}
