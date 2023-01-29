using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PictureAnalyzer.Data.Migrations
{
    public partial class AdjustAuthorizedUserProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Administrator",
                table: "AuthorizedUsers");

            migrationBuilder.DropColumn(
                name: "Delete",
                table: "AuthorizedUsers");

            migrationBuilder.DropColumn(
                name: "Read",
                table: "AuthorizedUsers");

            migrationBuilder.RenameColumn(
                name: "Write",
                table: "AuthorizedUsers",
                newName: "AllowEdit");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AllowEdit",
                table: "AuthorizedUsers",
                newName: "Write");

            migrationBuilder.AddColumn<bool>(
                name: "Administrator",
                table: "AuthorizedUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Delete",
                table: "AuthorizedUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Read",
                table: "AuthorizedUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
