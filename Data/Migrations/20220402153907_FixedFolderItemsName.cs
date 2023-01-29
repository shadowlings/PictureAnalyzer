using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PictureAnalyzer.Data.Migrations
{
    public partial class FixedFolderItemsName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_folderItems_Folders_FolderId",
                table: "folderItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_folderItems",
                table: "folderItems");

            migrationBuilder.RenameTable(
                name: "folderItems",
                newName: "FolderItems");

            migrationBuilder.RenameIndex(
                name: "IX_folderItems_FolderId",
                table: "FolderItems",
                newName: "IX_FolderItems_FolderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FolderItems",
                table: "FolderItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FolderItems_Folders_FolderId",
                table: "FolderItems",
                column: "FolderId",
                principalTable: "Folders",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FolderItems_Folders_FolderId",
                table: "FolderItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FolderItems",
                table: "FolderItems");

            migrationBuilder.RenameTable(
                name: "FolderItems",
                newName: "folderItems");

            migrationBuilder.RenameIndex(
                name: "IX_FolderItems_FolderId",
                table: "folderItems",
                newName: "IX_folderItems_FolderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_folderItems",
                table: "folderItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_folderItems_Folders_FolderId",
                table: "folderItems",
                column: "FolderId",
                principalTable: "Folders",
                principalColumn: "Id");
        }
    }
}
