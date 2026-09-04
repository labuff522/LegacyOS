using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegacyOS.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAutomaticSiblingDiscounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_automatic_sibling",
                table: "discount_codes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "sibling_end_position",
                table: "discount_codes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "sibling_start_position",
                table: "discount_codes",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_automatic_sibling",
                table: "discount_codes");

            migrationBuilder.DropColumn(
                name: "sibling_end_position",
                table: "discount_codes");

            migrationBuilder.DropColumn(
                name: "sibling_start_position",
                table: "discount_codes");
        }
    }
}
