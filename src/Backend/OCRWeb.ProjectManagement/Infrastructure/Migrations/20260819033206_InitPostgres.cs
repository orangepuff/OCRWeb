using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OCRWeb.ProjectManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "project");

            migrationBuilder.CreateTable(
                name: "Projects",
                schema: "project",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    iInsertedUserId = table.Column<int>(type: "integer", nullable: false),
                    dtInsertedTime = table.Column<DateTime>(type: "timestamp(3) without time zone", nullable: false),
                    iUpdatedUserId = table.Column<int>(type: "integer", nullable: true),
                    dtUpdatedTime = table.Column<DateTime>(type: "timestamp(3) without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Projects",
                schema: "project");
        }
    }
}
