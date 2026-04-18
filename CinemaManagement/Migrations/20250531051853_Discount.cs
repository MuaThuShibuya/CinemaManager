using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaManagement.Migrations
{
    /// <inheritdoc />
    public partial class Discount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DiscountEnd",
                table: "Showtimes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "DiscountPercent",
                table: "Showtimes",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DiscountStart",
                table: "Showtimes",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountEnd",
                table: "Showtimes");

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "Showtimes");

            migrationBuilder.DropColumn(
                name: "DiscountStart",
                table: "Showtimes");
        }
    }
}
