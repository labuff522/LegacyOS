using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegacyOS.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseRefunds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "refund_reason",
                table: "purchase_orders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "refunded_by_portal_user_id",
                table: "purchase_orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "refunded_on",
                table: "purchase_orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "stripe_payment_intent_id",
                table: "purchase_orders",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "stripe_refund_id",
                table: "purchase_orders",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "refund_reason",
                table: "purchase_orders");

            migrationBuilder.DropColumn(
                name: "refunded_by_portal_user_id",
                table: "purchase_orders");

            migrationBuilder.DropColumn(
                name: "refunded_on",
                table: "purchase_orders");

            migrationBuilder.DropColumn(
                name: "stripe_payment_intent_id",
                table: "purchase_orders");

            migrationBuilder.DropColumn(
                name: "stripe_refund_id",
                table: "purchase_orders");
        }
    }
}
