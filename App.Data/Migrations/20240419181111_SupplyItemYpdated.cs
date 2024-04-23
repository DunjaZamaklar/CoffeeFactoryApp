using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Data.Migrations
{
    /// <inheritdoc />
    public partial class SupplyItemYpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupplyItems_Suppliers_SupplierId",
                table: "SupplyItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplyItems_SupplyCategories_SupplyCategoryId",
                table: "SupplyItems");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "SupplyItems");

            migrationBuilder.RenameColumn(
                name: "SupplyCategoryId",
                table: "SupplyItems",
                newName: "SupplyOrderId");

            migrationBuilder.RenameColumn(
                name: "SupplierId",
                table: "SupplyItems",
                newName: "SupplyId");

            migrationBuilder.RenameIndex(
                name: "IX_SupplyItems_SupplyCategoryId",
                table: "SupplyItems",
                newName: "IX_SupplyItems_SupplyOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_SupplyItems_SupplierId",
                table: "SupplyItems",
                newName: "IX_SupplyItems_SupplyId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupplyItems_Supplies_SupplyId",
                table: "SupplyItems",
                column: "SupplyId",
                principalTable: "Supplies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplyItems_SupplyOrders_SupplyOrderId",
                table: "SupplyItems",
                column: "SupplyOrderId",
                principalTable: "SupplyOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupplyItems_Supplies_SupplyId",
                table: "SupplyItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplyItems_SupplyOrders_SupplyOrderId",
                table: "SupplyItems");

            migrationBuilder.RenameColumn(
                name: "SupplyOrderId",
                table: "SupplyItems",
                newName: "SupplyCategoryId");

            migrationBuilder.RenameColumn(
                name: "SupplyId",
                table: "SupplyItems",
                newName: "SupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_SupplyItems_SupplyOrderId",
                table: "SupplyItems",
                newName: "IX_SupplyItems_SupplyCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_SupplyItems_SupplyId",
                table: "SupplyItems",
                newName: "IX_SupplyItems_SupplierId");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "SupplyItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_SupplyItems_Suppliers_SupplierId",
                table: "SupplyItems",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplyItems_SupplyCategories_SupplyCategoryId",
                table: "SupplyItems",
                column: "SupplyCategoryId",
                principalTable: "SupplyCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
