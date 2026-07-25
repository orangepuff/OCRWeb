using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCRWeb.ProjectManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProjectsIdentityId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Projects.Id moves from Guid to int IDENTITY - same rationale as the earlier
            // Files.iId migration: existing rows can't be remapped onto a sequential identity
            // (approved data loss, dev-only data), and SQL Server can only assign IDENTITY at
            // column creation, never via ALTER COLUMN, so the column is dropped and recreated
            // rather than altered in place.
            migrationBuilder.Sql("DELETE FROM [project].[Projects];");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Projects",
                schema: "project",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "project",
                table: "Projects");

            migrationBuilder.AddColumn<int>(
                    name: "Id",
                    schema: "project",
                    table: "Projects",
                    type: "int",
                    nullable: false)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Projects",
                schema: "project",
                table: "Projects",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Projects",
                schema: "project",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "project",
                table: "Projects");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                schema: "project",
                table: "Projects",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Projects",
                schema: "project",
                table: "Projects",
                column: "Id");
        }
    }
}
