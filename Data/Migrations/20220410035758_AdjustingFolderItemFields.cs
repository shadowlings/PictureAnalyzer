using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PictureAnalyzer.Data.Migrations
{
    public partial class AdjustingFolderItemFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Path",
                table: "FolderItems",
                newName: "MimeType");

            migrationBuilder.RenameColumn(
                name: "FileType",
                table: "FolderItems",
                newName: "BlobStorageName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MimeType",
                table: "FolderItems",
                newName: "Path");

            migrationBuilder.RenameColumn(
                name: "BlobStorageName",
                table: "FolderItems",
                newName: "FileType");
        }
    }
}
