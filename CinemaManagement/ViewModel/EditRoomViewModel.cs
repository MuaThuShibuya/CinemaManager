using CinemaManagement.Models;
using System.Collections.Generic;

namespace CinemaManagement.ViewModels
{
    public class EditRoomViewModel
    {
        public int RoomId { get; set; }
        public int TheaterId { get; set; }

        public string RoomName { get; set; }
        public string Description { get; set; }

        public decimal? PriceMultiplier { get; set; }

        public List<Seat> Seats { get; set; } = new();
        public List<RoomSeatTypePrice> SeatTypePrices { get; set; } = new();
        public List<string> AllSeatTypes { get; set; } = new()
        {
            "Regular", "Vip", "Couple"
        };
    }
}
