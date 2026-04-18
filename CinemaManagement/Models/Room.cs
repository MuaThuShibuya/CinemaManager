// Models/Room.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CinemaManagement.Models
{
    public class Room
    {
        public int RoomId { get; set; }

        [Required]
        public int TheaterId { get; set; }     // FK → Theater

        [Required]
        public string Name { get; set; }

        public decimal? PriceMultiplier { get; set; }
        public string? Description { get; set; }

        // Navigation
        public Theater Theater { get; set; }
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();

        // Mỗi phòng có nhiều suất chiếu
        public ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
        
        // Trạng thái hoạt động của phòng
        public bool IsActive { get; set; } = true;
    }
}
