using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaManagement.Migrations
{
    /// <inheritdoc />
    public partial class DanhGiaTheoUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MovieRatings_MovieId",
                table: "MovieRatings");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "MovieRatings",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_MovieRatings_MovieId_UserId",
                table: "MovieRatings",
                columns: new[] { "MovieId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovieRatings_UserId",
                table: "MovieRatings",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovieRatings_AspNetUsers_UserId",
                table: "MovieRatings",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovieRatings_AspNetUsers_UserId",
                table: "MovieRatings");

            migrationBuilder.DropIndex(
                name: "IX_MovieRatings_MovieId_UserId",
                table: "MovieRatings");

            migrationBuilder.DropIndex(
                name: "IX_MovieRatings_UserId",
                table: "MovieRatings");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "MovieRatings");

            migrationBuilder.CreateIndex(
                name: "IX_MovieRatings_MovieId",
                table: "MovieRatings",
                column: "MovieId");
        }
    }
}
