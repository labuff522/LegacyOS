using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegacyOS.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProductPaymentPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "billing_day_of_month",
                table: "purchase_orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "installment_amount",
                table: "purchase_orders",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "installment_count",
                table: "purchase_orders",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<bool>(
                name: "is_payment_current",
                table: "purchase_orders",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "billing_day_of_month",
                table: "products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "installment_count",
                table: "products",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("UPDATE purchase_orders SET installment_amount = amount WHERE installment_amount = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "billing_day_of_month",
                table: "purchase_orders");

            migrationBuilder.DropColumn(
                name: "installment_amount",
                table: "purchase_orders");

            migrationBuilder.DropColumn(
                name: "installment_count",
                table: "purchase_orders");

            migrationBuilder.DropColumn(
                name: "is_payment_current",
                table: "purchase_orders");

            migrationBuilder.DropColumn(
                name: "billing_day_of_month",
                table: "products");

            migrationBuilder.DropColumn(
                name: "installment_count",
                table: "products");
        }
    }
}
