using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaManagement.Migrations
{
    public partial class RemoveTheaterMovieDirectRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // An toàn hơn khi kiểm tra xem constraint có tồn tại không
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Movies_Theaters_TheaterId'
                )
                ALTER TABLE Movies DROP CONSTRAINT FK_Movies_Theaters_TheaterId;
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.indexes WHERE name = 'IX_Movies_TheaterId'
                )
                DROP INDEX IX_Movies_TheaterId ON Movies;
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE Name = 'TheaterId' AND Object_ID = Object_ID('Movies')
                )
                ALTER TABLE Movies DROP COLUMN TheaterId;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TheaterId",
                table: "Movies",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Movies_TheaterId",
                table: "Movies",
                column: "TheaterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Movies_Theaters_TheaterId",
                table: "Movies",
                column: "TheaterId",
                principalTable: "Theaters",
                principalColumn: "TheaterId");
        }
    }
}
