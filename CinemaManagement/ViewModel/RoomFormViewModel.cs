using System.ComponentModel.DataAnnotations;
namespace CinemaManagement.ViewModels
{
    public class RoomFormViewModel
    {
        public int TheaterId { get; set; }
        
        public string RoomName { get; set; }
        [Required, Range(1, 300)] public int SeatCount { get; set; }
        public decimal? PriceMultiplier { get; set; }
        public string? Description { get; set; }
    }
}
