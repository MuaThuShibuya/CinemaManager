using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CinemaManagement.Models
{
    public class Genre
    {
        public int GenreId { get; set; }

        [Required]
        public string Name { get; set; }
        public List<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
    }
}
