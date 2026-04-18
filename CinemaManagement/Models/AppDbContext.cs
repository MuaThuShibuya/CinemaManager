using CinemaManagement.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Movie> Movies { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<MovieGenre> MovieGenres { get; set; }
    public DbSet<MovieRating> MovieRatings { get; set; }

    public DbSet<Theater> Theaters { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Showtime> Showtimes { get; set; }
    public DbSet<Seat> Seats { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Combo> Combos { get; set; }
    public DbSet<RoomSeatTypePrice> RoomSeatTypePrices { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // MovieGenre: composite key
        builder.Entity<MovieGenre>()
            .HasKey(mg => new { mg.MovieId, mg.GenreId });

        builder.Entity<MovieGenre>()
            .HasOne(mg => mg.Movie)
            .WithMany(m => m.MovieGenres)
            .HasForeignKey(mg => mg.MovieId);

        builder.Entity<MovieGenre>()
            .HasOne(mg => mg.Genre)
            .WithMany(g => g.MovieGenres)
            .HasForeignKey(mg => mg.GenreId);

        // Room → Theater (cascade delete)
        builder.Entity<Room>()
            .HasOne(r => r.Theater)
            .WithMany(t => t.Rooms)
            .HasForeignKey(r => r.TheaterId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seat → Room
        builder.Entity<Seat>()
            .HasOne(s => s.Room)
            .WithMany(r => r.Seats)
            .HasForeignKey(s => s.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        // Showtime → Movie
        builder.Entity<Showtime>()
            .HasOne(s => s.Movie)
            .WithMany(m => m.Showtimes)
            .HasForeignKey(s => s.MovieId)
            .OnDelete(DeleteBehavior.Cascade);

        // Showtime → Room
        builder.Entity<Showtime>()
            .HasOne(s => s.Room)
            .WithMany(r => r.Showtimes)
            .HasForeignKey(s => s.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ticket → Showtime
        builder.Entity<Ticket>()
            .HasOne(t => t.Showtime)
            .WithMany(s => s.Tickets)
            .HasForeignKey(t => t.ShowtimeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ticket → Seat
        builder.Entity<Ticket>()
            .HasOne(t => t.Seat)
            .WithMany(s => s.Tickets)
            .HasForeignKey(t => t.SeatId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ticket → ApplicationUser
        builder.Entity<Ticket>()
            .HasOne(t => t.User)
            .WithMany(u => u.Tickets)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure decimal precision for Ticket.Price
        builder.Entity<Ticket>()
            .Property(t => t.Price)
            .HasColumnType("decimal(18,2)");

        // MovieRating → ApplicationUser
        builder.Entity<MovieRating>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ràng buộc: 1 user chỉ được đánh giá 1 lần cho 1 phim
        builder.Entity<MovieRating>()
            .HasIndex(r => new { r.MovieId, r.UserId })
            .IsUnique();
    }
}
