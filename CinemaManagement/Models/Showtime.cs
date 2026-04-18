// Models/Showtime.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CinemaManagement.Models
{
    public class Showtime
    {
        public int ShowtimeId { get; set; }

        [Required(ErrorMessage = "Phim không được để trống")]
        public int MovieId { get; set; }

        [Required(ErrorMessage = "Phòng chiếu không được để trống")]
        public int RoomId { get; set; }    
        
        // Thêm cột Price
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(1, 99999999, ErrorMessage = "Giá phải từ 1 đến 99999999")]
        public decimal Price { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "Thời gian chiếu không được để trống")]
        public DateTime StartTime { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "Thời gian kết thúc không được để trống")]
        public DateTime EndTime { get; set; }

        //Cột giảm giá
        public decimal? DiscountPercent { get; set; } // VD: 20 = giảm 20%
        public DateTime? DiscountStart { get; set; }
        public DateTime? DiscountEnd { get; set; }

        // Navigation
        [ForeignKey("MovieId")]
        [ValidateNever]
        public Movie Movie { get; set; }

        [ForeignKey("RoomId")]
        [ValidateNever]
        public Room Room { get; set; }

        [ValidateNever]
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
