using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OCRWeb.Document.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "docproc");

            migrationBuilder.CreateTable(
                name: "Files",
                schema: "docproc",
                columns: table => new
                {
                    iId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    iParentId = table.Column<int>(type: "integer", nullable: false),
                    sFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    sContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    biSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    binChecksum = table.Column<byte[]>(type: "bytea", maxLength: 32, nullable: false),
                    iFileType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    btActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    iInsertedUserId = table.Column<int>(type: "integer", nullable: false),
                    dtInsertedTime = table.Column<DateTime>(type: "timestamp(3) without time zone", nullable: false),
                    iUpdatedUserId = table.Column<int>(type: "integer", nullable: true),
                    dtUpdatedTime = table.Column<DateTime>(type: "timestamp(3) without time zone", nullable: true),
                    sFileProperties = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.iId);
                });

            migrationBuilder.CreateTable(
                name: "FileContents",
                schema: "docproc",
                columns: table => new
                {
                    iId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    iFileId = table.Column<int>(type: "integer", nullable: false),
                    binContent = table.Column<byte[]>(type: "bytea", nullable: false),
                    iInsertedUserId = table.Column<int>(type: "integer", nullable: false),
                    dtInsertedTime = table.Column<DateTime>(type: "timestamp(3) without time zone", nullable: false),
                    iUpdatedUserId = table.Column<int>(type: "integer", nullable: true),
                    dtUpdatedTime = table.Column<DateTime>(type: "timestamp(3) without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileContents", x => x.iId);
                    table.ForeignKey(
                        name: "FK_FileContents_Files_iFileId",
                        column: x => x.iFileId,
                        principalSchema: "docproc",
                        principalTable: "Files",
                        principalColumn: "iId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileContents_iFileId",
                schema: "docproc",
                table: "FileContents",
                column: "iFileId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileContents",
                schema: "docproc");

            migrationBuilder.DropTable(
                name: "Files",
                schema: "docproc");
        }
    }
}
