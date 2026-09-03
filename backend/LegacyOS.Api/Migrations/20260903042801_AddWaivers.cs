using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegacyOS.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddWaivers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "waiver_templates",
                columns: table => new
                {
                    waiver_template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    file_content = table.Column<byte[]>(type: "bytea", nullable: false),
                    sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_waiver_templates", x => x.waiver_template_id);
                    table.ForeignKey(
                        name: "FK_waiver_templates_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "organization_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "waiver_signatures",
                columns: table => new
                {
                    waiver_signature_id = table.Column<Guid>(type: "uuid", nullable: false),
                    waiver_template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    athlete_id = table.Column<Guid>(type: "uuid", nullable: false),
                    guardian_id = table.Column<Guid>(type: "uuid", nullable: false),
                    portal_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    signed_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    waiver_sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ip_address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    signed_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_waiver_signatures", x => x.waiver_signature_id);
                    table.ForeignKey(
                        name: "FK_waiver_signatures_athletes_athlete_id",
                        column: x => x.athlete_id,
                        principalTable: "athletes",
                        principalColumn: "athlete_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_waiver_signatures_families_family_id",
                        column: x => x.family_id,
                        principalTable: "families",
                        principalColumn: "family_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_waiver_signatures_guardians_guardian_id",
                        column: x => x.guardian_id,
                        principalTable: "guardians",
                        principalColumn: "guardian_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_waiver_signatures_portal_users_portal_user_id",
                        column: x => x.portal_user_id,
                        principalTable: "portal_users",
                        principalColumn: "portal_user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_waiver_signatures_waiver_templates_waiver_template_id",
                        column: x => x.waiver_template_id,
                        principalTable: "waiver_templates",
                        principalColumn: "waiver_template_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_waiver_signatures_athlete_id",
                table: "waiver_signatures",
                column: "athlete_id");

            migrationBuilder.CreateIndex(
                name: "IX_waiver_signatures_family_id",
                table: "waiver_signatures",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "IX_waiver_signatures_guardian_id",
                table: "waiver_signatures",
                column: "guardian_id");

            migrationBuilder.CreateIndex(
                name: "IX_waiver_signatures_portal_user_id",
                table: "waiver_signatures",
                column: "portal_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_waiver_signatures_waiver_template_id_athlete_id",
                table: "waiver_signatures",
                columns: new[] { "waiver_template_id", "athlete_id" });

            migrationBuilder.CreateIndex(
                name: "IX_waiver_templates_organization_id_name_version",
                table: "waiver_templates",
                columns: new[] { "organization_id", "name", "version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "waiver_signatures");

            migrationBuilder.DropTable(
                name: "waiver_templates");
        }
    }
}
