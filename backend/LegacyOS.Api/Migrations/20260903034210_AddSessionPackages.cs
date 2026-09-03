using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegacyOS.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionPackages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "has_unlimited_sessions",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_session_package",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "session_count",
                table: "products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "validity_days",
                table: "products",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "session_credit_lots",
                columns: table => new
                {
                    session_credit_lot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    athlete_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    purchase_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_unlimited = table.Column<bool>(type: "boolean", nullable: false),
                    sessions_granted = table.Column<int>(type: "integer", nullable: true),
                    sessions_remaining = table.Column<int>(type: "integer", nullable: true),
                    granted_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_credit_lots", x => x.session_credit_lot_id);
                    table.ForeignKey(
                        name: "FK_session_credit_lots_athletes_athlete_id",
                        column: x => x.athlete_id,
                        principalTable: "athletes",
                        principalColumn: "athlete_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_session_credit_lots_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_session_credit_lots_purchase_orders_purchase_order_id",
                        column: x => x.purchase_order_id,
                        principalTable: "purchase_orders",
                        principalColumn: "purchase_order_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "session_ledger_entries",
                columns: table => new
                {
                    session_ledger_entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_credit_lot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    athlete_id = table.Column<Guid>(type: "uuid", nullable: false),
                    staff_portal_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    entry_type = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    session_change = table.Column<int>(type: "integer", nullable: false),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_ledger_entries", x => x.session_ledger_entry_id);
                    table.ForeignKey(
                        name: "FK_session_ledger_entries_session_credit_lots_session_credit_l~",
                        column: x => x.session_credit_lot_id,
                        principalTable: "session_credit_lots",
                        principalColumn: "session_credit_lot_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_session_credit_lots_athlete_id",
                table: "session_credit_lots",
                column: "athlete_id");

            migrationBuilder.CreateIndex(
                name: "IX_session_credit_lots_product_id",
                table: "session_credit_lots",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_session_credit_lots_purchase_order_id",
                table: "session_credit_lots",
                column: "purchase_order_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_session_ledger_entries_session_credit_lot_id",
                table: "session_ledger_entries",
                column: "session_credit_lot_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "session_ledger_entries");

            migrationBuilder.DropTable(
                name: "session_credit_lots");

            migrationBuilder.DropColumn(
                name: "has_unlimited_sessions",
                table: "products");

            migrationBuilder.DropColumn(
                name: "is_session_package",
                table: "products");

            migrationBuilder.DropColumn(
                name: "session_count",
                table: "products");

            migrationBuilder.DropColumn(
                name: "validity_days",
                table: "products");
        }
    }
}
