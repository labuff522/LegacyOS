using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegacyOS.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUsaWrestlingVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "usa_wrestling_verifications",
                columns: table => new
                {
                    usa_wrestling_verification_id = table.Column<Guid>(type: "uuid", nullable: false),
                    athlete_id = table.Column<Guid>(type: "uuid", nullable: false),
                    membership_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    submitted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    verified_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expires_on = table.Column<DateOnly>(type: "date", nullable: true),
                    verified_by_portal_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    staff_notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usa_wrestling_verifications", x => x.usa_wrestling_verification_id);
                    table.ForeignKey(
                        name: "FK_usa_wrestling_verifications_athletes_athlete_id",
                        column: x => x.athlete_id,
                        principalTable: "athletes",
                        principalColumn: "athlete_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usa_wrestling_verifications_portal_users_verified_by_portal~",
                        column: x => x.verified_by_portal_user_id,
                        principalTable: "portal_users",
                        principalColumn: "portal_user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_usa_wrestling_verifications_athlete_id",
                table: "usa_wrestling_verifications",
                column: "athlete_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usa_wrestling_verifications_membership_number",
                table: "usa_wrestling_verifications",
                column: "membership_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usa_wrestling_verifications_verified_by_portal_user_id",
                table: "usa_wrestling_verifications",
                column: "verified_by_portal_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "usa_wrestling_verifications");
        }
    }
}
