using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PictureAnalyzer.Data.Migrations
{
    public partial class AddVisionResults : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImageRatings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsAdultContent = table.Column<bool>(type: "bit", nullable: false),
                    IsRacyContent = table.Column<bool>(type: "bit", nullable: false),
                    IsGoryContent = table.Column<bool>(type: "bit", nullable: false),
                    AdultScore = table.Column<double>(type: "float", nullable: false),
                    RacyScore = table.Column<double>(type: "float", nullable: false),
                    GoreScore = table.Column<double>(type: "float", nullable: false),
                    FolderItemId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageRatings_FolderItems_FolderItemId",
                        column: x => x.FolderItemId,
                        principalTable: "FolderItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ImageTags",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Confidence = table.Column<double>(type: "float", nullable: false),
                    FolderItemId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImageTags_FolderItems_FolderItemId",
                        column: x => x.FolderItemId,
                        principalTable: "FolderItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImageRatings_FolderItemId",
                table: "ImageRatings",
                column: "FolderItemId",
                unique: true,
                filter: "[FolderItemId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ImageTags_FolderItemId",
                table: "ImageTags",
                column: "FolderItemId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImageRatings");

            migrationBuilder.DropTable(
                name: "ImageTags");
        }
    }
}
