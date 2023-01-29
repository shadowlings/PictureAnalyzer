using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PictureAnalyzer.Data.Migrations
{
    public partial class AddFileSizeToFolderItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "FolderItems",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "FolderItems");
        }
    }
}
