using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegacyOS.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountsAndOrderSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "athlete_snapshot_json",
                table: "purchase_orders",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "discount_amount",
                table: "purchase_orders",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "discount_code_id",
                table: "purchase_orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "discount_code_snapshot",
                table: "purchase_orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "discount_redemption_recorded",
                table: "purchase_orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "family_snapshot_json",
                table: "purchase_orders",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "item_snapshot_json",
                table: "purchase_orders",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "original_amount",
                table: "purchase_orders",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "discount_codes",
                columns: table => new
                {
                    discount_code_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    discount_type = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    value = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: true),
                    starts_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ends_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    max_redemptions = table.Column<int>(type: "integer", nullable: true),
                    redemption_count = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discount_codes", x => x.discount_code_id);
                    table.ForeignKey(
                        name: "FK_discount_codes_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_discount_code_id",
                table: "purchase_orders",
                column: "discount_code_id");

            migrationBuilder.CreateIndex(
                name: "IX_discount_codes_code",
                table: "discount_codes",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_discount_codes_product_id",
                table: "discount_codes",
                column: "product_id");

            migrationBuilder.AddForeignKey(
                name: "FK_purchase_orders_discount_codes_discount_code_id",
                table: "purchase_orders",
                column: "discount_code_id",
                principalTable: "discount_codes",
                principalColumn: "discount_code_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_purchase_orders_discount_codes_discount_code_id",
                table: "purchase_orders");

            migrationBuilder.DropTable(
                name: "discount_codes");

            migrationBuilder.DropIndex(
                name: "IX_purchase_orders_discount_code_id",
                table: "purchase_orders");

            migrationBuilder.DropColumn(
                name: "athlete_snapshot_json",
                table: "purchase_orders");

            migrationBuilder.DropColumn(
                name: "discount_amount",
                table: "purchase_orders");

            migrationBuilder.DropColumn(
                name: "discount_code_id",
                table: "purchase_orders");

            migrationBuilder.DropColumn(
                name: "discount_code_snapshot",
                table: "purchase_orders");

            migrationBuilder.DropColumn(
                name: "discount_redemption_recorded",
                table: "purchase_orders");

            migrationBuilder.DropColumn(
                name: "family_snapshot_json",
                table: "purchase_orders");

            migrationBuilder.DropColumn(
                name: "item_snapshot_json",
                table: "purchase_orders");

            migrationBuilder.DropColumn(
                name: "original_amount",
                table: "purchase_orders");
        }
    }
}
