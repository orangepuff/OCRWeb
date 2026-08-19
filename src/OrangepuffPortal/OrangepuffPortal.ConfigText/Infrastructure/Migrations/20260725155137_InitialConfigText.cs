using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrangepuffPortal.ConfigText.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialConfigText : Migration
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
                    iId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    sModule = table.Column<string>(type: "varchar(60)", nullable: false),
                    sTextCode = table.Column<string>(type: "varchar(60)", nullable: false),
                    sCultureCode = table.Column<string>(type: "varchar(10)", nullable: false),
                    sTextType = table.Column<string>(type: "varchar(10)", nullable: false),
                    sText = table.Column<string>(type: "nvarchar(1000)", nullable: false),
                    sNote = table.Column<string>(type: "nchar(255)", nullable: true),
                    iInsertedUserId = table.Column<int>(type: "int", nullable: true),
                    dtInsertedTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    iUpdatedUserId = table.Column<int>(type: "int", nullable: true),
                    dtUpdatedTime = table.Column<DateTime>(type: "datetime", nullable: true)
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
