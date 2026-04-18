using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaManagement.Migrations
{
    /// <inheritdoc />
    public partial class ChuaCoRoomID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Showtimes_ShowtimeId",
                table: "Rooms");

            migrationBuilder.DropForeignKey(
                name: "FK_Showtimes_Movies_MovieId",
                table: "Showtimes");

            migrationBuilder.DropForeignKey(
                name: "FK_Showtimes_Theaters_TheaterId",
                table: "Showtimes");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_ShowtimeId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "ShowtimeId",
                table: "Rooms");

            migrationBuilder.AddForeignKey(
                name: "FK_Showtimes_Movies_MovieId",
                table: "Showtimes",
                column: "MovieId",
                principalTable: "Movies",
                principalColumn: "MovieId");

            migrationBuilder.AddForeignKey(
                name: "FK_Showtimes_Theaters_TheaterId",
                table: "Showtimes",
                column: "TheaterId",
                principalTable: "Theaters",
                principalColumn: "TheaterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Showtimes_Movies_MovieId",
                table: "Showtimes");

            migrationBuilder.DropForeignKey(
                name: "FK_Showtimes_Theaters_TheaterId",
                table: "Showtimes");

            migrationBuilder.AddColumn<int>(
                name: "ShowtimeId",
                table: "Rooms",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_ShowtimeId",
                table: "Rooms",
                column: "ShowtimeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Showtimes_ShowtimeId",
                table: "Rooms",
                column: "ShowtimeId",
                principalTable: "Showtimes",
                principalColumn: "ShowtimeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Showtimes_Movies_MovieId",
                table: "Showtimes",
                column: "MovieId",
                principalTable: "Movies",
                principalColumn: "MovieId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Showtimes_Theaters_TheaterId",
                table: "Showtimes",
                column: "TheaterId",
                principalTable: "Theaters",
                principalColumn: "TheaterId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
