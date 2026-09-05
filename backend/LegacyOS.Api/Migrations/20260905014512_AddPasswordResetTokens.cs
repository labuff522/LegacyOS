using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegacyOS.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "portal_password_reset_tokens",
                columns: table => new
                {
                    portal_password_reset_token_id = table.Column<Guid>(type: "uuid", nullable: false),
                    portal_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    expires_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    used_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portal_password_reset_tokens", x => x.portal_password_reset_token_id);
                    table.ForeignKey(
                        name: "FK_portal_password_reset_tokens_portal_users_portal_user_id",
                        column: x => x.portal_user_id,
                        principalTable: "portal_users",
                        principalColumn: "portal_user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_portal_password_reset_tokens_portal_user_id",
                table: "portal_password_reset_tokens",
                column: "portal_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_portal_password_reset_tokens_token_hash",
                table: "portal_password_reset_tokens",
                column: "token_hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "portal_password_reset_tokens");
        }
    }
}
