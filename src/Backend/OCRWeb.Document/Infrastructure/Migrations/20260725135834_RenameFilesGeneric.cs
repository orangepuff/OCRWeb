using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCRWeb.Document.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameFilesGeneric : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PDFFileContents_PDFFiles_PdfFileId",
                schema: "docproc",
                table: "PDFFileContents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PDFFiles",
                schema: "docproc",
                table: "PDFFiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PDFFileContents",
                schema: "docproc",
                table: "PDFFileContents");

            migrationBuilder.RenameTable(
                name: "PDFFiles",
                schema: "docproc",
                newName: "Files",
                newSchema: "docproc");

            migrationBuilder.RenameTable(
                name: "PDFFileContents",
                schema: "docproc",
                newName: "FileContents",
                newSchema: "docproc");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "docproc",
                table: "Files",
                newName: "iId");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                schema: "docproc",
                table: "Files",
                newName: "iParentId");

            migrationBuilder.RenameColumn(
                name: "PdfFileId",
                schema: "docproc",
                table: "FileContents",
                newName: "iFileId");

            migrationBuilder.AddColumn<bool>(
                name: "btActive",
                schema: "docproc",
                table: "Files",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "iId",
                schema: "docproc",
                table: "FileContents",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Files",
                schema: "docproc",
                table: "Files",
                column: "iId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FileContents",
                schema: "docproc",
                table: "FileContents",
                column: "iId");

            migrationBuilder.CreateIndex(
                name: "IX_FileContents_iFileId",
                schema: "docproc",
                table: "FileContents",
                column: "iFileId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FileContents_Files_iFileId",
                schema: "docproc",
                table: "FileContents",
                column: "iFileId",
                principalSchema: "docproc",
                principalTable: "Files",
                principalColumn: "iId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileContents_Files_iFileId",
                schema: "docproc",
                table: "FileContents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Files",
                schema: "docproc",
                table: "Files");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FileContents",
                schema: "docproc",
                table: "FileContents");

            migrationBuilder.DropIndex(
                name: "IX_FileContents_iFileId",
                schema: "docproc",
                table: "FileContents");

            migrationBuilder.DropColumn(
                name: "btActive",
                schema: "docproc",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "iId",
                schema: "docproc",
                table: "FileContents");

            migrationBuilder.RenameTable(
                name: "Files",
                schema: "docproc",
                newName: "PDFFiles",
                newSchema: "docproc");

            migrationBuilder.RenameTable(
                name: "FileContents",
                schema: "docproc",
                newName: "PDFFileContents",
                newSchema: "docproc");

            migrationBuilder.RenameColumn(
                name: "iId",
                schema: "docproc",
                table: "PDFFiles",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "iParentId",
                schema: "docproc",
                table: "PDFFiles",
                newName: "ProjectId");

            migrationBuilder.RenameColumn(
                name: "iFileId",
                schema: "docproc",
                table: "PDFFileContents",
                newName: "PdfFileId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PDFFiles",
                schema: "docproc",
                table: "PDFFiles",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PDFFileContents",
                schema: "docproc",
                table: "PDFFileContents",
                column: "PdfFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_PDFFileContents_PDFFiles_PdfFileId",
                schema: "docproc",
                table: "PDFFileContents",
                column: "PdfFileId",
                principalSchema: "docproc",
                principalTable: "PDFFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
