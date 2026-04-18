using CinemaManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.Controllers
{
    [Authorize(Roles = "Admin,Employee")]
    public class TicketScanController : Controller
    {
        private readonly AppDbContext _context;

        public TicketScanController(AppDbContext context)
        {
            _context = context;
        }

        // Trang quét QR
        public IActionResult Scan()
        {
            return View();
        }

        // Xử lý sau khi quét
        public IActionResult Check(string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
            {
                ViewBag.Error = "OrderId không hợp lệ";
                return View("Result");
            }

            var tickets = _context.Tickets
                .Where(t => t.OrderId == orderId)
                .Include(t => t.Seat)
                    .ThenInclude(s => s.Room)
                        .ThenInclude(r => r.Theater)
                .Include(t => t.Showtime)
                    .ThenInclude(s => s.Movie)
                .ToList();

            if (!tickets.Any())
            {
                ViewBag.Error = "Vé không tồn tại";
                return View("Result");
            }

            if (tickets.Any(t => !t.IsPaid))
            {
                ViewBag.Error = "Vé chưa thanh toán";
                return View("Result");
            }

            if (tickets.Any(t => t.IsCheckedIn))
            {
                ViewBag.Error = "Vé đã được sử dụng";
                return View("Result");
            }

            // Đánh dấu đã soát
            foreach (var t in tickets)
            {
                t.IsCheckedIn = true;
            }

            _context.SaveChanges();

            return View("Result", tickets);
        }
        [HttpPost]
        public IActionResult AjaxCheck([FromBody] string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
                return Json(new { success = false, message = "OrderId không hợp lệ" });

            var tickets = _context.Tickets
                .Where(t => t.OrderId == orderId)
                .Include(t => t.Seat)
                    .ThenInclude(s => s.Room)
                        .ThenInclude(r => r.Theater)
                .Include(t => t.Showtime)
                    .ThenInclude(s => s.Movie)
                .ToList();

            if (!tickets.Any())
                return Json(new { success = false, message = "Vé không tồn tại" });

            if (tickets.Any(t => !t.IsPaid))
                return Json(new { success = false, message = "Vé chưa thanh toán" });

            if (tickets.Any(t => t.IsCheckedIn))
                return Json(new { success = false, message = "Vé đã được sử dụng" });

            foreach (var t in tickets)
                t.IsCheckedIn = true;

            _context.SaveChanges();

            return Json(new
            {
                success = true,
                tickets = tickets.Select(t => new
                {
                    movie = t.Showtime.Movie.Title,
                    theater = t.Seat.Room.Theater.Name,
                    room = t.Seat.Room.Name,
                    seat = t.Seat.SeatNumber,
                    time = t.Showtime.StartTime.ToString("dd/MM/yyyy HH:mm"),
                    payment = t.PaymentMethod
                })
            });
        }

    }
}
