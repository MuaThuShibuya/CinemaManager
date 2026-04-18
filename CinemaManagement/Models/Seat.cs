using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaManagement.Models
{
    public class Seat
    {
        [Key]
        public int SeatId { get; set; }

        public int RoomId { get; set; }

        public int SeatNumber { get; set; }

        public bool IsBooked { get; set; }

        // Lưu loại ghế dưới dạng string trong DB qua EF conversion (config trong DbContext)
        public SeatType SeatType { get; set; } = SeatType.Regular;
        public Room Room { get; set; }
        public ICollection<Ticket> Tickets { get; set; }
    }
}
