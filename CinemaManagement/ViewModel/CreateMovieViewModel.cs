using CinemaManagement.Models;

namespace CinemaManagement.ViewModel
{
    public class CreateMovieViewModel
    {
        public Movie Movie { get; set; }
        public List<Genre> Genres { get; set; }
        public List<Theater> Theaters { get; set; }
        public List<int> SelectedGenres { get; set; }
        public int SelectedTheaterId { get; set; }
    }

}
