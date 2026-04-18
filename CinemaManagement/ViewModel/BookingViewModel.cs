using CinemaManagement.Models;
using System.Collections.Generic;

namespace CinemaManagement.ViewModels
{
    public class BookingViewModel
    {
        public Movie Movie { get; set; }
        public List<Theater> Theaters { get; set; }
        public List<Showtime> Showtimes { get; set; }
        public List<int> SelectedSeatIds { get; set; }
        public List<Seat> Seats { get; set; }  // Thêm danh sách ghế vào đây
        public long TotalPrice { get; set; }

        public List<Room> Rooms { get; set; }  // Danh sách phòng chiếu
    }
}
