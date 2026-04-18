using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaManagement.Models
{
    public class Movie
    {
        public int MovieId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Range(1, 500, ErrorMessage = "Thời lượng phải từ 1 đến 500 phút")]
        public int Duration { get; set; }

        // ====== IMAGE ======
        public string? PosterUrl { get; set; }   // 2:3 hoặc 3:4
        public string? BannerUrl { get; set; }   // 16:9

        // ====== VIDEO ======
        [Url]
        public string? VideoUrl { get; set; }

        public DateTime ReleaseDate { get; set; }

        public double Rating { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        public ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>(); 
        public List<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
        public ICollection<MovieRating> MovieRatings { get; set; } = new List<MovieRating>();
    }
}