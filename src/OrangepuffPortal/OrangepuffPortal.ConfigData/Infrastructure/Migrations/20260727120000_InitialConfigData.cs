using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrangepuffPortal.ConfigData.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialConfigData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "configdata");

            migrationBuilder.CreateTable(
                name: "ConfigData",
                schema: "configdata",
                columns: table => new
                {
                    iId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    sKey = table.Column<string>(type: "varchar(100)", nullable: false),
                    sValue = table.Column<string>(type: "varchar(max)", nullable: true),
                    iInsertedUserId = table.Column<int>(type: "int", nullable: true),
                    dtInsertedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    iUpdatedUserId = table.Column<int>(type: "int", nullable: true),
                    dtUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    bAllowEditByScreen = table.Column<bool>(type: "bit", nullable: true),
                    sDescription = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigData", x => x.iId);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_ConfigData_Key",
                schema: "configdata",
                table: "ConfigData",
                column: "sKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfigData",
                schema: "configdata");
        }
    }
}
