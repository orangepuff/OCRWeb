using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCRWeb.DocumentProcessing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialDocumentProcessing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "docproc");

            migrationBuilder.CreateTable(
                name: "PDFFiles",
                schema: "docproc",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    sFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    sContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    biSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    binChecksum = table.Column<byte[]>(type: "varbinary(32)", maxLength: 32, nullable: false),
                    iFileType = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    iInsertedUserId = table.Column<int>(type: "int", nullable: false),
                    dtInsertedTime = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    iUpdatedUserId = table.Column<int>(type: "int", nullable: true),
                    dtUpdatedTime = table.Column<DateTime>(type: "datetime2(3)", nullable: true),
                    sFileProperties = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PDFFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PDFFileContents",
                schema: "docproc",
                columns: table => new
                {
                    PdfFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    binContent = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    iInsertedUserId = table.Column<int>(type: "int", nullable: false),
                    dtInsertedTime = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    iUpdatedUserId = table.Column<int>(type: "int", nullable: true),
                    dtUpdatedTime = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PDFFileContents", x => x.PdfFileId);
                    table.ForeignKey(
                        name: "FK_PDFFileContents_PDFFiles_PdfFileId",
                        column: x => x.PdfFileId,
                        principalSchema: "docproc",
                        principalTable: "PDFFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PDFFileContents",
                schema: "docproc");

            migrationBuilder.DropTable(
                name: "PDFFiles",
                schema: "docproc");
        }
    }
}
