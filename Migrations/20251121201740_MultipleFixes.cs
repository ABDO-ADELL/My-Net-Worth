using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRISM.Migrations
{
    /// <inheritdoc />
    public partial class MultipleFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_OrderId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Orders_BusinessId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Items_BusinessId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_BusinessId",
                table: "Expenses");

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Suppliers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "RevokeOn",
                table: "RefreshTokens",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_BusinessId",
                table: "Suppliers",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId_datetime_IsDeleted",
                table: "Payments",
                columns: new[] { "OrderId", "datetime", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BusinessId_datetime_IsDeleted",
                table: "Orders",
                columns: new[] { "BusinessId", "datetime", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Items_BusinessId_BranchId_IsDeleted",
                table: "Items",
                columns: new[] { "BusinessId", "BranchId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_BusinessId_ExpenseDate_IsDeleted",
                table: "Expenses",
                columns: new[] { "BusinessId", "ExpenseDate", "IsDeleted" });

            migrationBuilder.AddForeignKey(
                name: "FK_Suppliers_Businesses_BusinessId",
                table: "Suppliers",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "BusinessId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Suppliers_Businesses_BusinessId",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_BusinessId",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Payments_OrderId_datetime_IsDeleted",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Orders_BusinessId_datetime_IsDeleted",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Items_BusinessId_BranchId_IsDeleted",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_BusinessId_ExpenseDate_IsDeleted",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Suppliers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RevokeOn",
                table: "RefreshTokens",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BusinessId",
                table: "Orders",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_BusinessId",
                table: "Items",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_BusinessId",
                table: "Expenses",
                column: "BusinessId");
        }
    }
}
