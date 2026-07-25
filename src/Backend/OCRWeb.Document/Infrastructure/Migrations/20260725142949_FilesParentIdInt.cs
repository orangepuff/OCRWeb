using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCRWeb.Document.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FilesParentIdInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // iParentId tracks Projects.Id's own Guid->int migration. Guid<->int isn't a valid
            // SQL Server CAST/CONVERT pair, so ALTER COLUMN can't do this in place even though
            // Files is already empty (wiped by the earlier Files.iId migration).
            migrationBuilder.DropColumn(
                name: "iParentId",
                schema: "docproc",
                table: "Files");

            migrationBuilder.AddColumn<int>(
                name: "iParentId",
                schema: "docproc",
                table: "Files",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "iParentId",
                schema: "docproc",
                table: "Files");

            migrationBuilder.AddColumn<Guid>(
                name: "iParentId",
                schema: "docproc",
                table: "Files",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty);
        }
    }
}
