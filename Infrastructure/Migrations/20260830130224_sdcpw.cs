using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class sdcpw : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MissingOrderLine_Orders_OrderId",
                table: "MissingOrderLine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MissingOrderLine",
                table: "MissingOrderLine");

            migrationBuilder.DropIndex(
                name: "IX_MissingOrderLine_OrderId",
                table: "MissingOrderLine");

            migrationBuilder.AlterColumn<string>(
                name: "OrderId",
                table: "MissingOrderLine",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MissingOrderLine",
                table: "MissingOrderLine",
                columns: new[] { "OrderId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_MissingOrderLine_Orders_OrderId",
                table: "MissingOrderLine",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MissingOrderLine_Orders_OrderId",
                table: "MissingOrderLine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MissingOrderLine",
                table: "MissingOrderLine");

            migrationBuilder.AlterColumn<string>(
                name: "OrderId",
                table: "MissingOrderLine",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MissingOrderLine",
                table: "MissingOrderLine",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_MissingOrderLine_OrderId",
                table: "MissingOrderLine",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_MissingOrderLine_Orders_OrderId",
                table: "MissingOrderLine",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");
        }
    }
}
