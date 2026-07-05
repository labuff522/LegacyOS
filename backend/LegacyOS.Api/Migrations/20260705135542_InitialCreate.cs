using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegacyOS.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "families",
                columns: table => new
                {
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_families", x => x.family_id);
                });

            migrationBuilder.CreateTable(
                name: "athletes",
                columns: table => new
                {
                    athlete_id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: false),
                    gender = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_athletes", x => x.athlete_id);
                    table.ForeignKey(
                        name: "FK_athletes_families_family_id",
                        column: x => x.family_id,
                        principalTable: "families",
                        principalColumn: "family_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "guardians",
                columns: table => new
                {
                    guardian_id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    is_primary_contact = table.Column<bool>(type: "boolean", nullable: false),
                    receives_billing = table.Column<bool>(type: "boolean", nullable: false),
                    receives_sms = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guardians", x => x.guardian_id);
                    table.ForeignKey(
                        name: "FK_guardians_families_family_id",
                        column: x => x.family_id,
                        principalTable: "families",
                        principalColumn: "family_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_athletes_family_id",
                table: "athletes",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "IX_guardians_family_id",
                table: "guardians",
                column: "family_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "athletes");

            migrationBuilder.DropTable(
                name: "guardians");

            migrationBuilder.DropTable(
                name: "families");
        }
    }
}
