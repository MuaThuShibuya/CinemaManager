using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaManagement.Migrations
{
    /// <inheritdoc />
    public partial class PriceeShowtimes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Movies");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Showtimes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Showtimes");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Movies",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
