using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrangepuffPortal.Config.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "config");

            migrationBuilder.CreateTable(
                name: "ConfigSections",
                schema: "config",
                columns: table => new
                {
                    iId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    sModule = table.Column<string>(type: "varchar(60)", nullable: false),
                    sSectionDesc = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    sTextCode = table.Column<string>(type: "varchar(100)", nullable: false),
                    btShow = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    iInsertedUserId = table.Column<int>(type: "int", nullable: true),
                    dtInsertedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    iUpdatedUserId = table.Column<int>(type: "int", nullable: true),
                    dtUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigSections", x => x.iId);
                });

            migrationBuilder.CreateTable(
                name: "Configs",
                schema: "config",
                columns: table => new
                {
                    iId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    iSectionId = table.Column<int>(type: "int", nullable: false),
                    sConfigCode = table.Column<string>(type: "varchar(60)", nullable: false),
                    sConfigName = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    sTextCode = table.Column<string>(type: "varchar(100)", nullable: false),
                    iConfigType = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    btShow = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    btAllowUserEdit = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    iInsertedUserId = table.Column<int>(type: "int", nullable: true),
                    dtInsertedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    iUpdatedUserId = table.Column<int>(type: "int", nullable: true),
                    dtUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configs", x => x.iId);
                    table.ForeignKey(
                        name: "FK_Configs_ConfigSections_iSectionId",
                        column: x => x.iSectionId,
                        principalSchema: "config",
                        principalTable: "ConfigSections",
                        principalColumn: "iId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConfigUsers",
                schema: "config",
                columns: table => new
                {
                    iId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    iUserId = table.Column<int>(type: "int", nullable: false),
                    iConfigId = table.Column<int>(type: "int", nullable: false),
                    sConfigValue = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    iConfigValue = table.Column<int>(type: "int", nullable: true),
                    nConfigValue = table.Column<decimal>(type: "decimal(8,3)", nullable: true),
                    btConfigValue = table.Column<bool>(type: "bit", nullable: true),
                    btActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    iInsertedUserId = table.Column<int>(type: "int", nullable: false),
                    dtInsertedTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    iUpdatedUserId = table.Column<int>(type: "int", nullable: true),
                    dtUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigUsers", x => x.iId);
                    table.ForeignKey(
                        name: "FK_ConfigUsers_Configs_iConfigId",
                        column: x => x.iConfigId,
                        principalSchema: "config",
                        principalTable: "Configs",
                        principalColumn: "iId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConfigUsersHistory",
                schema: "config",
                columns: table => new
                {
                    iId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    iConfigUserId = table.Column<int>(type: "int", nullable: false),
                    iUserId = table.Column<int>(type: "int", nullable: false),
                    iConfigId = table.Column<int>(type: "int", nullable: false),
                    sConfigValue = table.Column<string>(type: "nvarchar(255)", nullable: true),
                    iConfigValue = table.Column<int>(type: "int", nullable: true),
                    nConfigValue = table.Column<decimal>(type: "decimal(8,3)", nullable: true),
                    btConfigValue = table.Column<bool>(type: "bit", nullable: true),
                    btActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    iInsertedUserId = table.Column<int>(type: "int", nullable: false),
                    dtInsertedTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    dtUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    iUpdatedUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigUsersHistory", x => x.iId);
                    table.ForeignKey(
                        name: "FK_ConfigUsersHistory_ConfigUsers_iConfigUserId",
                        column: x => x.iConfigUserId,
                        principalSchema: "config",
                        principalTable: "ConfigUsers",
                        principalColumn: "iId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Configs_iSectionId",
                schema: "config",
                table: "Configs",
                column: "iSectionId");

            migrationBuilder.CreateIndex(
                name: "UQ_Configs_ConfigCode",
                schema: "config",
                table: "Configs",
                column: "sConfigCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ConfigSections_Module_TextCode",
                schema: "config",
                table: "ConfigSections",
                columns: new[] { "sModule", "sTextCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConfigUsers_iConfigId",
                schema: "config",
                table: "ConfigUsers",
                column: "iConfigId");

            migrationBuilder.CreateIndex(
                name: "UQ_ConfigUsers_User_Config",
                schema: "config",
                table: "ConfigUsers",
                columns: new[] { "iUserId", "iConfigId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConfigUsersHistory_ConfigUserId",
                schema: "config",
                table: "ConfigUsersHistory",
                column: "iConfigUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfigUsersHistory",
                schema: "config");

            migrationBuilder.DropTable(
                name: "ConfigUsers",
                schema: "config");

            migrationBuilder.DropTable(
                name: "Configs",
                schema: "config");

            migrationBuilder.DropTable(
                name: "ConfigSections",
                schema: "config");
        }
    }
}
