using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PictureAnalyzer.Data.Migrations
{
    public partial class RestructingFoldersAndPermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FolderPermissions");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Folders");

            migrationBuilder.CreateTable(
                name: "AuthorizedUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Administrator = table.Column<bool>(type: "bit", nullable: false),
                    Read = table.Column<bool>(type: "bit", nullable: false),
                    Write = table.Column<bool>(type: "bit", nullable: false),
                    Delete = table.Column<bool>(type: "bit", nullable: false),
                    FolderId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizedUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthorizedUsers_Folders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "Folders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "folderItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrettyFileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FolderId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_folderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_folderItems_Folders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "Folders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizedUsers_FolderId",
                table: "AuthorizedUsers",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_folderItems_FolderId",
                table: "folderItems",
                column: "FolderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorizedUsers");

            migrationBuilder.DropTable(
                name: "folderItems");

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Folders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FolderPermissions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FolderId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AllowEdit = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsOwner = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FolderPermissions_Folders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "Folders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FolderId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Images_Folders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "Folders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FolderPermissions_FolderId",
                table: "FolderPermissions",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_FolderId",
                table: "Images",
                column: "FolderId");
        }
    }
}
