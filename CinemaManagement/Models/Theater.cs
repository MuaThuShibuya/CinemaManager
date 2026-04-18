using System.ComponentModel.DataAnnotations;

namespace CinemaManagement.Models
{
    public class Theater
    {
        public int TheaterId { get; set; }

        [Required(ErrorMessage = "Tên rạp là bắt buộc")]
        public string Name { get; set; }

        public string Location { get; set; }

        // Thêm thuộc tính City
        [Required(ErrorMessage = "Thành phố là bắt buộc")]
        public string City { get; set; }

        public ICollection<Room> Rooms { get; set; } = new List<Room>();
        
        // Trạng thái hoạt động của rạp
        public bool IsActive { get; set; } = true;
    }
}
