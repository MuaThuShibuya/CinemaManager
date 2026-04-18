using CinemaManagement.Models;
using System;
using System.Collections.Generic;

namespace CinemaManagement.ViewModels
{
    public class SeatSelectionViewModel
    {
        public Showtime Showtime { get; set; }
        public Movie Movie { get; set; }
        public Room Room { get; set; }

        public List<Seat> AvailableSeats { get; set; }

        public int ShowtimeId => Showtime?.ShowtimeId ?? 0;
        public int RoomId => Room?.RoomId ?? 0;

        // GIÁ
        public decimal BasePrice { get; set; }        // Giá gốc suất chiếu (đã nhân multiplier)
        public decimal PriceRegular { get; set; }     // Giá phụ loại ghế
        public decimal PriceVip { get; set; }
        public decimal PriceCouple { get; set; }

        // GIẢM GIÁ
        public decimal? DiscountPercent { get; set; }
        public DateTime? DiscountStart { get; set; }
        public DateTime? DiscountEnd { get; set; }

        // GHẾ
        public List<int> SelectedSeats { get; set; } = new();
        public List<int> PaidSeatIds { get; set; } = new();
        public List<int> ReservedSeatIds { get; set; } = new();
    }
}
