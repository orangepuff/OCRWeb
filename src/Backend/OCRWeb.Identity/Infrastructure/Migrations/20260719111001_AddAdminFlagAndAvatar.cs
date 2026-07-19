using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCRWeb.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminFlagAndAvatar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "btAdmin",
                schema: "identity",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "UserAvatars",
                schema: "identity",
                columns: table => new
                {
                    iUserId = table.Column<int>(type: "int", nullable: false),
                    binAvatar = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    sContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    dtInsertedTime = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    dtUpdatedTime = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAvatars", x => x.iUserId);
                    table.ForeignKey(
                        name: "FK_UserAvatars_Users_iUserId",
                        column: x => x.iUserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "iId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAvatars",
                schema: "identity");

            migrationBuilder.DropColumn(
                name: "btAdmin",
                schema: "identity",
                table: "Users");
        }
    }
}
