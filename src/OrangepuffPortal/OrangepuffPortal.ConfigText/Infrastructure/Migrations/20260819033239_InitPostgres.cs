using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OrangepuffPortal.ConfigText.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "configtext");

            migrationBuilder.CreateTable(
                name: "ConfigTextDefinition",
                schema: "configtext",
                columns: table => new
                {
                    iId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sModule = table.Column<string>(type: "varchar(60)", nullable: false),
                    sTextCode = table.Column<string>(type: "varchar(60)", nullable: false),
                    sCultureCode = table.Column<string>(type: "varchar(10)", nullable: false),
                    sTextType = table.Column<string>(type: "varchar(10)", nullable: false),
                    sText = table.Column<string>(type: "varchar(1000)", nullable: false),
                    sNote = table.Column<string>(type: "char(255)", nullable: true),
                    iInsertedUserId = table.Column<int>(type: "integer", nullable: true),
                    dtInsertedTime = table.Column<DateTime>(type: "timestamp", nullable: true),
                    iUpdatedUserId = table.Column<int>(type: "integer", nullable: true),
                    dtUpdatedTime = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigTextDefinition", x => x.iId);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_ConfigTextDefinition_Module_Code_Culture_Type",
                schema: "configtext",
                table: "ConfigTextDefinition",
                columns: new[] { "sModule", "sTextCode", "sCultureCode", "sTextType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfigTextDefinition",
                schema: "configtext");
        }
    }
}
