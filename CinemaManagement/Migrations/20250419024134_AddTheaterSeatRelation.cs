using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddTheaterSeatRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Seats_TheaterId",
                table: "Seats",
                column: "TheaterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Seats_Theaters_TheaterId",
                table: "Seats",
                column: "TheaterId",
                principalTable: "Theaters",
                principalColumn: "TheaterId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Seats_Theaters_TheaterId",
                table: "Seats");

            migrationBuilder.DropIndex(
                name: "IX_Seats_TheaterId",
                table: "Seats");
        }
    }
}
