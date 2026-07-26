using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCRWeb.Document.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FilesIdentityId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Files.iId moves from Guid to int IDENTITY - existing rows' key values can't be
            // remapped onto a sequential identity, and this repo's dev DB has no data worth
            // preserving here, so both tables are wiped before the columns are dropped and
            // recreated (SQL Server can only set IDENTITY at column creation, never via ALTER
            // COLUMN, and a Guid->int ALTER COLUMN would fail on any existing rows anyway).
            migrationBuilder.Sql("DELETE FROM [docproc].[FileContents];");
            migrationBuilder.Sql("DELETE FROM [docproc].[Files];");

            migrationBuilder.DropForeignKey(
                name: "FK_FileContents_Files_iFileId",
                schema: "docproc",
                table: "FileContents");

            migrationBuilder.DropIndex(
                name: "IX_FileContents_iFileId",
                schema: "docproc",
                table: "FileContents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Files",
                schema: "docproc",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "iId",
                schema: "docproc",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "iFileId",
                schema: "docproc",
                table: "FileContents");

            migrationBuilder.AddColumn<int>(
                    name: "iId",
                    schema: "docproc",
                    table: "Files",
                    type: "int",
                    nullable: false)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "iFileId",
                schema: "docproc",
                table: "FileContents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Files",
                schema: "docproc",
                table: "Files",
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

            migrationBuilder.DropIndex(
                name: "IX_FileContents_iFileId",
                schema: "docproc",
                table: "FileContents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Files",
                schema: "docproc",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "iId",
                schema: "docproc",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "iFileId",
                schema: "docproc",
                table: "FileContents");

            migrationBuilder.AddColumn<Guid>(
                name: "iId",
                schema: "docproc",
                table: "Files",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AddColumn<Guid>(
                name: "iFileId",
                schema: "docproc",
                table: "FileContents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Files",
                schema: "docproc",
                table: "Files",
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
    }
}
