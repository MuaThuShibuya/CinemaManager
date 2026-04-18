using CinemaManagement.Models;
using System.Collections.Generic;

namespace CinemaManagement.ViewModel
{
    public class TheaterMoviesViewModel
    {
        public List<Movie> Movies { get; set; }
        public List<Movie> NowShowingMovies { get; set; }
        public List<Movie> ComingSoonMovies { get; set; }

        // Đổi sang tên phòng chiếu (Room)
        public string RoomName { get; set; }

        public int? RoomId { get; set; }

        // Nếu có dùng TheaterId thì giữ, nếu không có dùng thì bỏ cũng được
        public int? TheaterId { get; set; }
        public string TheaterName { get; set; }
        public bool IsFiltered { get; set; }

        // Danh sách phòng chiếu để dropdown lọc
        public List<Room> Rooms { get; set; }

        public string SearchQuery { get; set; }
    }
}
