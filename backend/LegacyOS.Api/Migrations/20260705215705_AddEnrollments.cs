using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegacyOS.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEnrollments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "enrollments",
                columns: table => new
                {
                    enrollment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    athlete_id = table.Column<Guid>(type: "uuid", nullable: false),
                    membership_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enrollments", x => x.enrollment_id);
                    table.ForeignKey(
                        name: "FK_enrollments_athletes_athlete_id",
                        column: x => x.athlete_id,
                        principalTable: "athletes",
                        principalColumn: "athlete_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_enrollments_membership_plans_membership_plan_id",
                        column: x => x.membership_plan_id,
                        principalTable: "membership_plans",
                        principalColumn: "membership_plan_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_enrollments_athlete_id",
                table: "enrollments",
                column: "athlete_id");

            migrationBuilder.CreateIndex(
                name: "IX_enrollments_membership_plan_id",
                table: "enrollments",
                column: "membership_plan_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "enrollments");
        }
    }
}
