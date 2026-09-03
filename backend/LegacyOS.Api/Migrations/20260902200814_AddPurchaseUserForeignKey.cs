using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegacyOS.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseUserForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_portal_user_id",
                table: "purchase_orders",
                column: "portal_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_purchase_orders_portal_users_portal_user_id",
                table: "purchase_orders",
                column: "portal_user_id",
                principalTable: "portal_users",
                principalColumn: "portal_user_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_purchase_orders_portal_users_portal_user_id",
                table: "purchase_orders");

            migrationBuilder.DropIndex(
                name: "IX_purchase_orders_portal_user_id",
                table: "purchase_orders");
        }
    }
}
