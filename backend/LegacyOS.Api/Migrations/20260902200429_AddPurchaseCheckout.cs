using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegacyOS.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseCheckout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "purchase_orders",
                columns: table => new
                {
                    purchase_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    portal_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    athlete_id = table.Column<Guid>(type: "uuid", nullable: true),
                    membership_plan_id = table.Column<Guid>(type: "uuid", nullable: true),
                    product_id = table.Column<Guid>(type: "uuid", nullable: true),
                    enrollment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    kind = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    status = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    stripe_checkout_session_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    stripe_customer_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    stripe_subscription_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_orders", x => x.purchase_order_id);
                    table.ForeignKey(
                        name: "FK_purchase_orders_athletes_athlete_id",
                        column: x => x.athlete_id,
                        principalTable: "athletes",
                        principalColumn: "athlete_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchase_orders_enrollments_enrollment_id",
                        column: x => x.enrollment_id,
                        principalTable: "enrollments",
                        principalColumn: "enrollment_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchase_orders_families_family_id",
                        column: x => x.family_id,
                        principalTable: "families",
                        principalColumn: "family_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchase_orders_membership_plans_membership_plan_id",
                        column: x => x.membership_plan_id,
                        principalTable: "membership_plans",
                        principalColumn: "membership_plan_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchase_orders_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_athlete_id",
                table: "purchase_orders",
                column: "athlete_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_enrollment_id",
                table: "purchase_orders",
                column: "enrollment_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_family_id",
                table: "purchase_orders",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_membership_plan_id",
                table: "purchase_orders",
                column: "membership_plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_product_id",
                table: "purchase_orders",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_stripe_checkout_session_id",
                table: "purchase_orders",
                column: "stripe_checkout_session_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "purchase_orders");
        }
    }
}
