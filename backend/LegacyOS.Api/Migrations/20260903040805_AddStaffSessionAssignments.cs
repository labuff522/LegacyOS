using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegacyOS.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffSessionAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "purchase_order_id",
                table: "session_credit_lots",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "grant_source",
                table: "session_credit_lots",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Stripe");

            migrationBuilder.AddColumn<Guid>(
                name: "granted_by_staff_portal_user_id",
                table: "session_credit_lots",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "grant_source",
                table: "session_credit_lots");

            migrationBuilder.DropColumn(
                name: "granted_by_staff_portal_user_id",
                table: "session_credit_lots");

            migrationBuilder.AlterColumn<Guid>(
                name: "purchase_order_id",
                table: "session_credit_lots",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
