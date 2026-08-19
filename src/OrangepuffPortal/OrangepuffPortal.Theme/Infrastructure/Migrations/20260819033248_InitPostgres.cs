using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OrangepuffPortal.Theme.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "portal");

            migrationBuilder.CreateTable(
                name: "Themes",
                schema: "portal",
                columns: table => new
                {
                    iId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sThemeCode = table.Column<string>(type: "varchar(100)", nullable: false),
                    sDescription = table.Column<string>(type: "varchar(500)", nullable: false),
                    btActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    iInsertedUserId = table.Column<int>(type: "integer", nullable: true),
                    dtInsertedTime = table.Column<DateTime>(type: "timestamp(3) without time zone", nullable: false),
                    iUpdatedUserId = table.Column<int>(type: "integer", nullable: true),
                    dtUpdatedTime = table.Column<DateTime>(type: "timestamp(3) without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Themes", x => x.iId);
                });

            migrationBuilder.CreateTable(
                name: "ThemeSections",
                schema: "portal",
                columns: table => new
                {
                    iId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    iThemeId = table.Column<int>(type: "integer", nullable: false),
                    sSectionCode = table.Column<string>(type: "varchar(100)", nullable: false),
                    sDescription = table.Column<string>(type: "varchar(500)", nullable: false),
                    iSortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    iInsertedUserId = table.Column<int>(type: "integer", nullable: true),
                    dtInsertedTime = table.Column<DateTime>(type: "timestamp(3) without time zone", nullable: false),
                    iUpdatedUserId = table.Column<int>(type: "integer", nullable: true),
                    dtUpdatedTime = table.Column<DateTime>(type: "timestamp(3) without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThemeSections", x => x.iId);
                    table.ForeignKey(
                        name: "FK_ThemeSections_Themes_iThemeId",
                        column: x => x.iThemeId,
                        principalSchema: "portal",
                        principalTable: "Themes",
                        principalColumn: "iId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThemeElements",
                schema: "portal",
                columns: table => new
                {
                    iId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    iThemeSectionId = table.Column<int>(type: "integer", nullable: false),
                    sElementCode = table.Column<string>(type: "varchar(100)", nullable: false),
                    sDescription = table.Column<string>(type: "varchar(500)", nullable: false),
                    iSortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    iInsertedUserId = table.Column<int>(type: "integer", nullable: true),
                    dtInsertedTime = table.Column<DateTime>(type: "timestamp(3) without time zone", nullable: false),
                    iUpdatedUserId = table.Column<int>(type: "integer", nullable: true),
                    dtUpdatedTime = table.Column<DateTime>(type: "timestamp(3) without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThemeElements", x => x.iId);
                    table.ForeignKey(
                        name: "FK_ThemeElements_ThemeSections_iThemeSectionId",
                        column: x => x.iThemeSectionId,
                        principalSchema: "portal",
                        principalTable: "ThemeSections",
                        principalColumn: "iId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThemeDetails",
                schema: "portal",
                columns: table => new
                {
                    iId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    iThemeElementId = table.Column<int>(type: "integer", nullable: false),
                    sPropertyKey = table.Column<string>(type: "varchar(100)", nullable: false),
                    sPropertyLabel = table.Column<string>(type: "varchar(200)", nullable: false),
                    sPropertyDescription = table.Column<string>(type: "varchar(500)", nullable: false),
                    sPropertyType = table.Column<string>(type: "varchar(50)", nullable: false),
                    sAllowedValues = table.Column<string>(type: "varchar(1000)", nullable: true),
                    sPropertyValue = table.Column<string>(type: "varchar(500)", nullable: true),
                    sUnit = table.Column<string>(type: "varchar(20)", nullable: true),
                    iSortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    iInsertedUserId = table.Column<int>(type: "integer", nullable: true),
                    dtInsertedTime = table.Column<DateTime>(type: "timestamp(3) without time zone", nullable: false),
                    iUpdatedUserId = table.Column<int>(type: "integer", nullable: true),
                    dtUpdatedTime = table.Column<DateTime>(type: "timestamp(3) without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThemeDetails", x => x.iId);
                    table.ForeignKey(
                        name: "FK_ThemeDetails_ThemeElements_iThemeElementId",
                        column: x => x.iThemeElementId,
                        principalSchema: "portal",
                        principalTable: "ThemeElements",
                        principalColumn: "iId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_ThemeDetails_ElementId_PropertyKey",
                schema: "portal",
                table: "ThemeDetails",
                columns: new[] { "iThemeElementId", "sPropertyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ThemeElements_SectionId_ElementCode",
                schema: "portal",
                table: "ThemeElements",
                columns: new[] { "iThemeSectionId", "sElementCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Themes_ThemeCode",
                schema: "portal",
                table: "Themes",
                column: "sThemeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ThemeSections_ThemeId_SectionCode",
                schema: "portal",
                table: "ThemeSections",
                columns: new[] { "iThemeId", "sSectionCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThemeDetails",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "ThemeElements",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "ThemeSections",
                schema: "portal");

            migrationBuilder.DropTable(
                name: "Themes",
                schema: "portal");
        }
    }
}
