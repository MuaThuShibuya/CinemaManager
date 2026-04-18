using System;
using System.ComponentModel.DataAnnotations;

namespace CinemaManagement.Models
{
    public class ShowtimeViewModel
    {
        public int ShowtimeId { get; set; }

        public int MovieId { get; set; }

        [Display(Name = "Rạp")]
        [Required(ErrorMessage = "Phải chọn rạp")]
        public int TheaterId { get; set; }

        [Display(Name = "Phòng")]
        [Required(ErrorMessage = "Phải chọn phòng")]
        public int RoomId { get; set; }

        [Display(Name = "Thời gian bắt đầu")]
        [Required(ErrorMessage = "Phải nhập thời gian bắt đầu")]
        public DateTime StartTime { get; set; }

        // ⚠ EndTime được tính tự động trong Controller
        [Display(Name = "Thời gian kết thúc")]
        public DateTime EndTime { get; set; }

        [Display(Name = "Giá tiền")]
        [Range(1, 99999999, ErrorMessage = "Giá phải từ 1 đến 99999999")]
        public decimal Price { get; set; }

        [Display(Name = "Giảm giá (%)")]
        [Range(0, 100, ErrorMessage = "Giảm giá từ 0 đến 100%")]
        public decimal? DiscountPercent { get; set; }

        [Display(Name = "Bắt đầu giảm giá")]
        public DateTime? DiscountStart { get; set; }

        [Display(Name = "Kết thúc giảm giá")]
        public DateTime? DiscountEnd { get; set; }
    }
}
