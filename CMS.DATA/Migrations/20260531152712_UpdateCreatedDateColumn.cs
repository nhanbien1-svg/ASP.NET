using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMS.DATA.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCreatedDateColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Categories_CategoryID",
                table: "Posts");

            migrationBuilder.RenameColumn(
                name: "CategoryID",
                table: "Posts",
                newName: "CategoryId");

            migrationBuilder.RenameColumn(
                name: "CreatedData",
                table: "Posts",
                newName: "CreatedDate");

            migrationBuilder.RenameIndex(
                name: "IX_Posts_CategoryID",
                table: "Posts",
                newName: "IX_Posts_CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Categories_CategoryId",
                table: "Posts",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Categories_CategoryId",
                table: "Posts");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Posts",
                newName: "CategoryID");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Posts",
                newName: "CreatedData");

            migrationBuilder.RenameIndex(
                name: "IX_Posts_CategoryId",
                table: "Posts",
                newName: "IX_Posts_CategoryID");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Categories_CategoryID",
                table: "Posts",
                column: "CategoryID",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
