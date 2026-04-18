using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaManagement.Models
{
    public class Ticket
    {
        public int TicketId { get; set; }

        // Khoá ngoại đến bảng AspNetUsers
        public string UserId { get; set; }

        public int ShowtimeId { get; set; }
        public int SeatId { get; set; }

        public string OrderId { get; set; }
        public decimal Price { get; set; }
        public string PaymentMethod { get; set; }  // "Momo" hoặc "Cash"
        public bool IsCheckedIn { get; set; } = false;
        public bool IsPaid { get; set; } = false;
        public DateTime BookingTime { get; set; } = DateTime.Now;

        // Navigation properties
        public ApplicationUser User { get; set; }
        public Showtime Showtime { get; set; }
        public Seat Seat { get; set; }
    }
}
