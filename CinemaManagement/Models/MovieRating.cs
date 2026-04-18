namespace CinemaManagement.Models
{
    public class MovieRating
    {
        public int MovieRatingId { get; set; }
        public int MovieId { get; set; }
        public int Rating { get; set; }
        public DateTime RatedAt { get; set; }
        public Movie Movie { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
