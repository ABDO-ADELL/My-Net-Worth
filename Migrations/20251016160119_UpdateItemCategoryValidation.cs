using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRISM.Migrations
{
    /// <inheritdoc />
    public partial class UpdateItemCategoryValidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_ItemCategories_ItemCategoryProductCategoryId",
                table: "Items");

            migrationBuilder.RenameColumn(
                name: "ItemCategoryProductCategoryId",
                table: "Items",
                newName: "ItemCategoryCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Items_ItemCategoryProductCategoryId",
                table: "Items",
                newName: "IX_Items_ItemCategoryCategoryId");

            migrationBuilder.RenameColumn(
                name: "ProductCategoryId",
                table: "ItemCategories",
                newName: "CategoryId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ItemCategories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_ItemCategories_ItemCategoryCategoryId",
                table: "Items",
                column: "ItemCategoryCategoryId",
                principalTable: "ItemCategories",
                principalColumn: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_ItemCategories_ItemCategoryCategoryId",
                table: "Items");

            migrationBuilder.RenameColumn(
                name: "ItemCategoryCategoryId",
                table: "Items",
                newName: "ItemCategoryProductCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Items_ItemCategoryCategoryId",
                table: "Items",
                newName: "IX_Items_ItemCategoryProductCategoryId");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "ItemCategories",
                newName: "ProductCategoryId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ItemCategories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_ItemCategories_ItemCategoryProductCategoryId",
                table: "Items",
                column: "ItemCategoryProductCategoryId",
                principalTable: "ItemCategories",
                principalColumn: "ProductCategoryId");
        }
    }
}
