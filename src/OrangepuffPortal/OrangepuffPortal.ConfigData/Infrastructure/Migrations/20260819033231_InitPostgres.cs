using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OrangepuffPortal.ConfigData.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitPostgres : Migration
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
                    iId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sKey = table.Column<string>(type: "varchar(100)", nullable: false),
                    sValue = table.Column<string>(type: "text", nullable: true),
                    iInsertedUserId = table.Column<int>(type: "integer", nullable: true),
                    dtInsertedTime = table.Column<DateTime>(type: "timestamp", nullable: true),
                    iUpdatedUserId = table.Column<int>(type: "integer", nullable: true),
                    dtUpdatedTime = table.Column<DateTime>(type: "timestamp", nullable: true),
                    bAllowEditByScreen = table.Column<bool>(type: "boolean", nullable: true),
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
