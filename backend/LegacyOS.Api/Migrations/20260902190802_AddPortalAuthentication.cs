using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegacyOS.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPortalAuthentication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "guardian_invitations",
                columns: table => new
                {
                    guardian_invitation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    guardian_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    expires_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    accepted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guardian_invitations", x => x.guardian_invitation_id);
                    table.ForeignKey(
                        name: "FK_guardian_invitations_guardians_guardian_id",
                        column: x => x.guardian_id,
                        principalTable: "guardians",
                        principalColumn: "guardian_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "portal_users",
                columns: table => new
                {
                    portal_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    normalized_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    role = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    guardian_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portal_users", x => x.portal_user_id);
                    table.ForeignKey(
                        name: "FK_portal_users_guardians_guardian_id",
                        column: x => x.guardian_id,
                        principalTable: "guardians",
                        principalColumn: "guardian_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "portal_access_tokens",
                columns: table => new
                {
                    portal_access_token_id = table.Column<Guid>(type: "uuid", nullable: false),
                    portal_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    expires_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portal_access_tokens", x => x.portal_access_token_id);
                    table.ForeignKey(
                        name: "FK_portal_access_tokens_portal_users_portal_user_id",
                        column: x => x.portal_user_id,
                        principalTable: "portal_users",
                        principalColumn: "portal_user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_guardian_invitations_guardian_id",
                table: "guardian_invitations",
                column: "guardian_id");

            migrationBuilder.CreateIndex(
                name: "IX_guardian_invitations_token_hash",
                table: "guardian_invitations",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_portal_access_tokens_portal_user_id",
                table: "portal_access_tokens",
                column: "portal_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_portal_access_tokens_token_hash",
                table: "portal_access_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_portal_users_guardian_id",
                table: "portal_users",
                column: "guardian_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_portal_users_normalized_email",
                table: "portal_users",
                column: "normalized_email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "guardian_invitations");

            migrationBuilder.DropTable(
                name: "portal_access_tokens");

            migrationBuilder.DropTable(
                name: "portal_users");
        }
    }
}
