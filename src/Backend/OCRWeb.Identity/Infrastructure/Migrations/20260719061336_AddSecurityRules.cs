using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCRWeb.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "btIsTemplateUser",
                schema: "identity",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "iParentId",
                schema: "identity",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SecurityRuleCategory",
                schema: "identity",
                columns: table => new
                {
                    iId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    sCategoryDesc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    sTextCode = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    btHidden = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    iInsertedUserId = table.Column<int>(type: "int", nullable: false),
                    dtInsertedTime = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    iUpdatedUserId = table.Column<int>(type: "int", nullable: true),
                    dtUpdatedTime = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityRuleCategory", x => x.iId);
                });

            migrationBuilder.CreateTable(
                name: "SecurityRuleItems",
                schema: "identity",
                columns: table => new
                {
                    iId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    iRuleCategoryId = table.Column<int>(type: "int", nullable: false),
                    sSecurityRuleCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    sSecurityRuleDesc = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    iRuleType = table.Column<int>(type: "int", nullable: false),
                    iSortOrder = table.Column<int>(type: "int", nullable: true),
                    sTextCode = table.Column<string>(type: "nvarchar(90)", maxLength: 90, nullable: true),
                    btHidden = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    iInsertedUserId = table.Column<int>(type: "int", nullable: false),
                    dtInsertedTime = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    iUpdatedUserId = table.Column<int>(type: "int", nullable: true),
                    dtUpdatedTime = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityRuleItems", x => x.iId);
                    table.ForeignKey(
                        name: "FK_SecurityRuleItems_SecurityRuleCategory_iRuleCategoryId",
                        column: x => x.iRuleCategoryId,
                        principalSchema: "identity",
                        principalTable: "SecurityRuleCategory",
                        principalColumn: "iId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SecurityUserRuleItems",
                schema: "identity",
                columns: table => new
                {
                    iId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    iUserId = table.Column<int>(type: "int", nullable: false),
                    iRuleItemId = table.Column<int>(type: "int", nullable: false),
                    iAllowed = table.Column<int>(type: "int", nullable: true),
                    nAllowed = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    iInsertedUserId = table.Column<int>(type: "int", nullable: false),
                    dtInsertedTime = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    iUpdatedUserId = table.Column<int>(type: "int", nullable: true),
                    dtUpdatedTime = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityUserRuleItems", x => x.iId);
                    table.ForeignKey(
                        name: "FK_SecurityUserRuleItems_SecurityRuleItems_iRuleItemId",
                        column: x => x.iRuleItemId,
                        principalSchema: "identity",
                        principalTable: "SecurityRuleItems",
                        principalColumn: "iId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SecurityUserRuleItems_Users_iUserId",
                        column: x => x.iUserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "iId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_iParentId",
                schema: "identity",
                table: "Users",
                column: "iParentId");

            migrationBuilder.CreateIndex(
                name: "UQ_SecurityRuleCategory_CategoryDesc",
                schema: "identity",
                table: "SecurityRuleCategory",
                column: "sCategoryDesc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SecurityRuleItems_iRuleCategoryId",
                schema: "identity",
                table: "SecurityRuleItems",
                column: "iRuleCategoryId");

            migrationBuilder.CreateIndex(
                name: "UQ_SecurityRuleItems_Code",
                schema: "identity",
                table: "SecurityRuleItems",
                column: "sSecurityRuleCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SecurityUserRuleItems_iRuleItemId",
                schema: "identity",
                table: "SecurityUserRuleItems",
                column: "iRuleItemId");

            migrationBuilder.CreateIndex(
                name: "UQ_SecurityUserRuleItems_UserId_RuleItemId",
                schema: "identity",
                table: "SecurityUserRuleItems",
                columns: new[] { "iUserId", "iRuleItemId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_iParentId",
                schema: "identity",
                table: "Users",
                column: "iParentId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "iId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_iParentId",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropTable(
                name: "SecurityUserRuleItems",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "SecurityRuleItems",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "SecurityRuleCategory",
                schema: "identity");

            migrationBuilder.DropIndex(
                name: "IX_Users_iParentId",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "btIsTemplateUser",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "iParentId",
                schema: "identity",
                table: "Users");
        }
    }
}
