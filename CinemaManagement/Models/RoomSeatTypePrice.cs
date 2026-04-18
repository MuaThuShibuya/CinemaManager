using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaManagement.Models
{
    public class RoomSeatTypePrice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RoomId { get; set; }
        public Room Room { get; set; }

        [Required]
        [MaxLength(20)]
        public string SeatType { get; set; } 

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
    }
}
