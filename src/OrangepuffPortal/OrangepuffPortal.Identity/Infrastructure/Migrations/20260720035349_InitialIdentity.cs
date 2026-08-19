using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrangepuffPortal.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity");

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
                name: "Users",
                schema: "identity",
                columns: table => new
                {
                    iId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    sUsername = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    sEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    sDisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    sPasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    btActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    btTemplateUser = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    iParentId = table.Column<int>(type: "int", nullable: true),
                    btAdmin = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    dtInsertedTime = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    dtUpdatedTime = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.iId);
                    table.ForeignKey(
                        name: "FK_Users_Users_iParentId",
                        column: x => x.iParentId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "iId",
                        onDelete: ReferentialAction.Restrict);
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
                name: "ExternalLogins",
                schema: "identity",
                columns: table => new
                {
                    iId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    iUserId = table.Column<int>(type: "int", nullable: false),
                    sProvider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    sProviderKey = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    dtInsertedTime = table.Column<DateTime>(type: "datetime2(3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalLogins", x => x.iId);
                    table.ForeignKey(
                        name: "FK_ExternalLogins_Users_iUserId",
                        column: x => x.iUserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "iId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAvatars",
                schema: "identity",
                columns: table => new
                {
                    iUserId = table.Column<int>(type: "int", nullable: false),
                    binAvatar = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    sContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    dtInsertedTime = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    dtUpdatedTime = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAvatars", x => x.iUserId);
                    table.ForeignKey(
                        name: "FK_UserAvatars_Users_iUserId",
                        column: x => x.iUserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "iId",
                        onDelete: ReferentialAction.Cascade);
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
                name: "UQ_ExternalLogins_Provider_ProviderKey",
                schema: "identity",
                table: "ExternalLogins",
                columns: new[] { "sProvider", "sProviderKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ExternalLogins_UserId_Provider",
                schema: "identity",
                table: "ExternalLogins",
                columns: new[] { "iUserId", "sProvider" },
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_Users_iParentId",
                schema: "identity",
                table: "Users",
                column: "iParentId");

            migrationBuilder.CreateIndex(
                name: "UQ_Users_Username",
                schema: "identity",
                table: "Users",
                column: "sUsername",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalLogins",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "SecurityUserRuleItems",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "UserAvatars",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "SecurityRuleItems",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "SecurityRuleCategory",
                schema: "identity");
        }
    }
}
