using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaManagement.Migrations
{
    /// <inheritdoc />
    public partial class Room : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Thêm cột Price vào Movies
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Movies",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            // 2. Tạo bảng Rooms trước
            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    RoomId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TheaterId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PriceMultiplier = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShowtimeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.RoomId);
                    table.ForeignKey(
                        name: "FK_Rooms_Showtimes_ShowtimeId",
                        column: x => x.ShowtimeId,
                        principalTable: "Showtimes",
                        principalColumn: "ShowtimeId");
                    table.ForeignKey(
                        name: "FK_Rooms_Theaters_TheaterId",
                        column: x => x.TheaterId,
                        principalTable: "Theaters",
                        principalColumn: "TheaterId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_ShowtimeId",
                table: "Rooms",
                column: "ShowtimeId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_TheaterId",
                table: "Rooms",
                column: "TheaterId");

            // 3. Sau khi đã có Rooms, tiến hành đổi cột trong Seats
            migrationBuilder.DropForeignKey(
                name: "FK_Seats_Theaters_TheaterId",
                table: "Seats");

            migrationBuilder.RenameColumn(
                name: "TheaterId",
                table: "Seats",
                newName: "RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_Seats_TheaterId",
                table: "Seats",
                newName: "IX_Seats_RoomId");

            // 4. Cuối cùng, thêm FK từ Seats.RoomId -> Rooms.RoomId
            migrationBuilder.AddForeignKey(
                name: "FK_Seats_Rooms_RoomId",
                table: "Seats",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "RoomId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
       name: "Price",
       table: "Movies");

            migrationBuilder.DropForeignKey(
                name: "FK_Seats_Rooms_RoomId",
                table: "Seats");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.RenameColumn(
                name: "RoomId",
                table: "Seats",
                newName: "TheaterId");

            migrationBuilder.RenameIndex(
                name: "IX_Seats_RoomId",
                table: "Seats",
                newName: "IX_Seats_TheaterId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Movies",
                type: "decimal(18,0)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddForeignKey(
                name: "FK_Seats_Theaters_TheaterId",
                table: "Seats",
                column: "TheaterId",
                principalTable: "Theaters",
                principalColumn: "TheaterId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
