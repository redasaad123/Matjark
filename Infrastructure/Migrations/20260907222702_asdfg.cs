using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class asdfg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ShopifyInventoryItemId",
                table: "Products",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ShopifyProductId",
                table: "Products",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ShopifyVariantId",
                table: "Products",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ShopifyOrderId",
                table: "Orders",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShopifyOrderNumber",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShopifyInventoryItemId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ShopifyProductId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ShopifyVariantId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ShopifyOrderId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShopifyOrderNumber",
                table: "Orders");
        }
    }
}
