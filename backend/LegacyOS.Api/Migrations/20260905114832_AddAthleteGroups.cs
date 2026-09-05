using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegacyOS.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAthleteGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "athlete_group_id",
                table: "athletes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "athlete_groups",
                columns: table => new
                {
                    athlete_group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_athlete_groups", x => x.athlete_group_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_athletes_athlete_group_id",
                table: "athletes",
                column: "athlete_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_athlete_groups_name",
                table: "athlete_groups",
                column: "name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_athletes_athlete_groups_athlete_group_id",
                table: "athletes",
                column: "athlete_group_id",
                principalTable: "athlete_groups",
                principalColumn: "athlete_group_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_athletes_athlete_groups_athlete_group_id",
                table: "athletes");

            migrationBuilder.DropTable(
                name: "athlete_groups");

            migrationBuilder.DropIndex(
                name: "IX_athletes_athlete_group_id",
                table: "athletes");

            migrationBuilder.DropColumn(
                name: "athlete_group_id",
                table: "athletes");
        }
    }
}
