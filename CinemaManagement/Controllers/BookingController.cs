using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaManagement.Data;
using CinemaManagement.Models;
using CinemaManagement.Services;
using CinemaManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using QRCoder;
using System.Drawing.Imaging;
using System.Drawing;

namespace CinemaManagement.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly MomoService _momoService;
        private readonly EmailService _emailService;
        private readonly ILogger<BookingController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private const int ReservationMinutes = 5; // Thời gian tạm khóa ghế

        public BookingController(
            AppDbContext context,
            MomoService momoService,
            ILogger<BookingController> logger,
            EmailService emailService,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _momoService = momoService;
            _logger = logger;
            _emailService = emailService;
            _userManager = userManager;
        }

        // STEP 1 — DANH SÁCH SUẤT CHIẾU
        [AllowAnonymous]
        public async Task<IActionResult> Index(int? movieId)
        {
            if (!movieId.HasValue)
                return NotFound("Không tìm thấy phim!");

            var showtimes = await _context.Showtimes
                .Where(s => s.MovieId == movieId)
                .Include(s => s.Movie)
                .Include(s => s.Room)
                    .ThenInclude(r => r.Theater)
                .ToListAsync();

            if (!showtimes.Any())
                return NotFound("Không tìm thấy suất chiếu nào!");

            return View(new BookingViewModel
            {
                Movie = showtimes[0].Movie,
                Showtimes = showtimes
            });
        }

        // STEP 2 — CHỌN GHẾ
        public async Task<IActionResult> SelectSeats(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Chặn Admin
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["Error"] = "Quản trị viên không thể mua vé.";
                return RedirectToAction("Index", "Movies");
            }

            var showtime = await _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Room)
                .FirstOrDefaultAsync(s => s.ShowtimeId == id);

            if (showtime == null)
                return NotFound("Lịch chiếu không tồn tại");

            var seats = await _context.Seats
                .Where(s => s.RoomId == showtime.RoomId)
                .Select(s => new Seat
                {
                    SeatId = s.SeatId,
                    SeatNumber = s.SeatNumber,
                    SeatType = s.SeatType,
                    IsBooked = false
                })
                .ToListAsync();

            var paidSeatIds = await _context.Tickets
                .Where(t => t.ShowtimeId == id && t.IsPaid)
                .Select(t => t.SeatId)
                .ToListAsync();

            var cutoff = DateTime.Now.AddMinutes(-ReservationMinutes);
            var reservedSeatIds = await _context.Tickets
                .Where(t => t.ShowtimeId == id && !t.IsPaid && t.BookingTime >= cutoff)
                .Select(t => t.SeatId)
                .ToListAsync();

            // Giá vé
            decimal basePrice = showtime.Price * (showtime.Room.PriceMultiplier ?? 1);

            var seatTypePrices = await _context.RoomSeatTypePrices
                .Where(p => p.RoomId == showtime.RoomId)
                .ToListAsync();

            decimal priceRegular = seatTypePrices.FirstOrDefault(x => x.SeatType == "Regular")?.Price ?? 0;
            decimal priceVip = seatTypePrices.FirstOrDefault(x => x.SeatType == "Vip")?.Price ?? 0;
            decimal priceCouple = seatTypePrices.FirstOrDefault(x => x.SeatType == "Couple")?.Price ?? 0;

            return View(new SeatSelectionViewModel
            {
                Showtime = showtime,
                Movie = showtime.Movie,
                Room = showtime.Room,
                AvailableSeats = seats,

                BasePrice = basePrice,
                PriceRegular = priceRegular,
                PriceVip = priceVip,
                PriceCouple = priceCouple,

                DiscountPercent = showtime.DiscountPercent,
                DiscountStart = showtime.DiscountStart,
                DiscountEnd = showtime.DiscountEnd,

                PaidSeatIds = paidSeatIds,
                ReservedSeatIds = reservedSeatIds
            });
        }

        // STEP 3 — XÁC NHẬN ĐẶT GHẾ
        [HttpPost]
        public async Task<IActionResult> ConfirmBooking([FromBody] BookingRequest req)
        {
            if (req?.SeatIds == null || !req.SeatIds.Any())
                return Json(new { success = false, error = "Chưa chọn ghế" });

            var showtime = await _context.Showtimes
                .Include(s => s.Room)
                .FirstOrDefaultAsync(s => s.ShowtimeId == req.ShowtimeId);

            if (showtime == null)
                return Json(new { success = false, error = "Suất chiếu không hợp lệ" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // KIỂM TRA GHẾ ĐÃ GIỮ
            var cutoff = DateTime.Now.AddMinutes(-ReservationMinutes);
            var takenSeats = await _context.Tickets
                .Where(t => t.ShowtimeId == req.ShowtimeId &&
                    (t.IsPaid || (!t.IsPaid && t.BookingTime >= cutoff)))
                .Select(t => t.SeatId)
                .ToListAsync();

            if (req.SeatIds.Any(id => takenSeats.Contains(id)))
                return Json(new { success = false, error = "Ghế đã được đặt" });

            decimal basePrice = showtime.Price * (showtime.Room.PriceMultiplier ?? 1);

            var seatTypePrices = await _context.RoomSeatTypePrices
                .Where(p => p.RoomId == showtime.RoomId)
                .ToListAsync();

            bool discountActive =
                showtime.DiscountPercent.HasValue &&
                showtime.DiscountStart <= DateTime.Now &&
                showtime.DiscountEnd >= DateTime.Now;

            decimal discountMultiplier = discountActive
                ? 1 - showtime.DiscountPercent.Value / 100m
                : 1m;

            string orderId = $"ORDER_{Guid.NewGuid():N}".Substring(0, 20);
            DateTime bookingTime = DateTime.Now;

            foreach (var seatId in req.SeatIds)
            {
                var seat = await _context.Seats.FindAsync(seatId);

                decimal seatExtra = seat.SeatType switch
                {
                    SeatType.Vip => seatTypePrices
                        .FirstOrDefault(x => x.SeatType == "Vip")?.Price ?? 0,

                    SeatType.Couple => seatTypePrices
                        .FirstOrDefault(x => x.SeatType == "Couple")?.Price ?? 0,

                    _ => seatTypePrices
                        .FirstOrDefault(x => x.SeatType == "Regular")?.Price ?? 0
                };

                decimal finalPrice = (basePrice + seatExtra) * discountMultiplier;

                _context.Tickets.Add(new Ticket
                {
                    UserId = userId,
                    ShowtimeId = showtime.ShowtimeId,
                    SeatId = seatId,
                    BookingTime = bookingTime,
                    Price = Math.Round(finalPrice),
                    IsPaid = false,
                    OrderId = orderId,
                    PaymentMethod = "Pending"
                });
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                redirectUrl = Url.Action("SelectCombos", new { orderId })
            });
        }

        // STEP 3.5 — CHỌN COMBO
        public async Task<IActionResult> SelectCombos(string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
                return NotFound("OrderId không hợp lệ!");

            var tickets = await _context.Tickets
                .Where(t => t.OrderId == orderId)
                .ToListAsync();

            if (!tickets.Any())
                return NotFound("Không tìm thấy vé!");

            decimal ticketTotal = tickets.Sum(t => t.Price);

            var availableCombos = await _context.Combos
                .Where(c => c.IsAvailable)
                .Select(c => new ComboItem
                {
                    Id = c.ComboId,
                    Name = c.Name,
                    Description = c.Description,
                    Price = c.Price,
                    ImageUrl = c.ImageUrl
                })
                .ToListAsync();

            var viewModel = new ComboSelectionViewModel
            {
                OrderId = orderId,
                TicketTotal = ticketTotal,
                AvailableCombos = availableCombos,
                ComboTotal = 0,
                GrandTotal = ticketTotal
            };

            return View(viewModel);
        }

        // STEP 3.6 — LƯU COMBO
        [HttpPost]
        public IActionResult SaveCombos([FromBody] ComboRequest req)
        {
            if (string.IsNullOrEmpty(req.OrderId) || req.SelectedCombos == null)
                return Json(new { success = false, error = "Dữ liệu không hợp lệ" });

            HttpContext.Session.SetString($"Combos_{req.OrderId}", JsonConvert.SerializeObject(req.SelectedCombos));
            HttpContext.Session.SetString($"ComboTotal_{req.OrderId}", req.ComboTotal.ToString());

            return Json(new { success = true });
        }

        // STEP 3.7 — THANH TOÁN
        public async Task<IActionResult> Payment(string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
                return NotFound("OrderId không hợp lệ!");

            var tickets = await _context.Tickets
                .Where(t => t.OrderId == orderId)
                .ToListAsync();

            if (!tickets.Any())
                return NotFound("Không tìm thấy vé!");

            decimal ticketTotal = tickets.Sum(t => t.Price);
            decimal comboTotal = 0;
            var comboTotalStr = HttpContext.Session.GetString($"ComboTotal_{orderId}");
            if (!string.IsNullOrEmpty(comboTotalStr))
                decimal.TryParse(comboTotalStr, out comboTotal);

            decimal grandTotal = ticketTotal + comboTotal;
            HttpContext.Session.SetString("TotalAmount", ((long)grandTotal).ToString());

            var viewModel = new PaymentViewModel
            {
                OrderId = orderId,
                TicketTotal = ticketTotal,
                ComboTotal = comboTotal,
                GrandTotal = grandTotal
            };

            return View(viewModel);
        }

        // STEP 4 — MOMO QR CODE
        [HttpPost]
        public async Task<IActionResult> GetMomoQRCode([FromBody] PaymentRequest req)
        {
            if (req == null || req.Amount <= 0)
                return BadRequest(new { success = false, error = "Dữ liệu không hợp lệ!" });

            string qrUrl = await _momoService.GeneratePaymentQRCode(req.Amount, req.OrderId);

            var tickets = await _context.Tickets.Where(t => t.OrderId == req.OrderId).ToListAsync();
            foreach (var t in tickets)
                t.PaymentMethod = "Pending";

            await _context.SaveChangesAsync();

            return Ok(new { success = true, qrUrl });
        }

        // STEP 5 — MOMO NOTIFY
        [AllowAnonymous]
        [HttpPost("/api/momo/notify")]
        public async Task<IActionResult> MomoNotifyUrl()
        {
            string body;

            using (var reader = new StreamReader(Request.Body))
                body = await reader.ReadToEndAsync();

            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);
            if (data == null || !data.ContainsKey("orderId"))
                return BadRequest();
            //Khời tạo orderID
            var orderId = data["orderId"];
            string qrBase64 = GenerateQRCode($"OrderId={orderId}");

            // 1. THANH TOÁN THÀNH CÔNG
            if (data.TryGetValue("resultCode", out var resultCode) && resultCode == "0")
            {
                var ticketsToPay = await _context.Tickets
                    .Where(t => t.OrderId == orderId && !t.IsPaid)
                    .Include(t => t.Seat)
                    .ToListAsync();

                foreach (var t in ticketsToPay)
                {
                    t.IsPaid = true;
                    t.PaymentMethod = "Momo";
                    if (t.Seat != null)
                        t.Seat.IsBooked = true;
                }

                await _context.SaveChangesAsync();
            }

            // 2. LẤY VÉ ĐÃ THANH TOÁN ĐỂ GỬI EMAIL
            var tickets = await _context.Tickets
                .Where(t => t.OrderId == orderId && t.IsPaid)
                .Include(t => t.User)
                .Include(t => t.Seat)
                    .ThenInclude(s => s.Room)
                        .ThenInclude(r => r.Theater)
                .Include(t => t.Showtime)
                    .ThenInclude(s => s.Movie)
                .ToListAsync();

            if (!tickets.Any())
                return Ok();

            var email = tickets.First().User?.Email;
            if (string.IsNullOrEmpty(email))
                return Ok();

            // 3. TẠO HTML EMAIL (ĐƠN GIẢN – GIỐNG VIEW)
            var sb = new System.Text.StringBuilder();

            sb.Append("<h2>🎉 Thanh toán thành công</h2>");
            sb.Append($"<p><b>Mã đơn:</b> {orderId}</p>");
            sb.Append("<hr/>");

            foreach (var t in tickets)
            {
                sb.Append("<div style='border:1px solid #ddd;padding:10px;margin-bottom:10px'>");
                sb.Append($"<p><b>Phim:</b> {t.Showtime?.Movie?.Title}</p>");
                sb.Append($"<p><b>Rạp:</b> {t.Seat?.Room?.Theater?.Name}</p>");
                sb.Append($"<p><b>Phòng:</b> {t.Seat?.Room?.Name}</p>");
                sb.Append($"<p><b>Ghế:</b> {t.Seat?.SeatNumber}</p>");
                sb.Append($"<p><b>Ngày:</b> {t.Showtime?.StartTime:dd/MM/yyyy}</p>");
                sb.Append($"<p><b>Giờ:</b> {t.Showtime?.StartTime:HH:mm}</p>");
                sb.Append($"<p><b>Giá:</b> {t.Price:#,##0} VND</p>");
                sb.Append("</div>");
            }

            var htmlContent = sb.ToString();

            // 4. GỬI EMAIL (KHỚP CHỮ KÝ EmailService)
            try
            {
                await _emailService.SendTicketEmailAsync(
                    email,
                    "Xác nhận đặt vé xem phim",
                    htmlContent,
                    orderId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Send email failed for OrderId {OrderId}", orderId);
            }

            return Ok();
        }

        // STEP 6 — PAYMENT SUCCESS
        [AllowAnonymous]
        public async Task<IActionResult> PaymentSuccess(string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
                return RedirectToAction("Index", "Movies");

            var tickets = await _context.Tickets
                .Include(t => t.User)
                .Include(t => t.Seat)
                    .ThenInclude(s => s.Room)
                        .ThenInclude(r => r.Theater)
                .Include(t => t.Showtime)
                    .ThenInclude(s => s.Movie)
                .Where(t => t.OrderId == orderId)
                .ToListAsync();

            if (!tickets.Any())
                return RedirectToAction("Index", "Movies");

            if (tickets.Any(t => !t.IsPaid))
                return View("PaymentPending", orderId);

            return View(new PaymentSuccessViewModel
            {
                Tickets = tickets,
                QrCodeBase64 = GenerateQRCode($"OrderId={orderId}")
            });
        }

        // STEP 7 — GHẾ ĐÃ THANH TOÁN
        [HttpGet]
        public async Task<IActionResult> GetPaidSeats(int showtimeId)
        {
            var paid = await _context.Tickets
                .Where(t => t.ShowtimeId == showtimeId && t.IsPaid)
                .Select(t => new { t.SeatId, t.PaymentMethod })
                .ToListAsync();

            return Json(new { paidSeats = paid });
        }

        private string GenerateQRCode(string content)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(qrData);
            using var ms = new MemoryStream();
            using var bitmap = qrCode.GetGraphic(20);

            bitmap.Save(ms, ImageFormat.Png);
            return Convert.ToBase64String(ms.ToArray());
        }

        [Authorize(Roles = "Employee")]
        [HttpPost]
        public async Task<IActionResult> CashPayment([FromBody] CashPaymentRequest req)
        {
            if (string.IsNullOrEmpty(req.OrderId))
                return Json(new { success = false, error = "OrderId không hợp lệ" });

            var tickets = await _context.Tickets
                .Where(t => t.OrderId == req.OrderId && !t.IsPaid)
                .Include(t => t.Seat)
                .ToListAsync();

            foreach (var t in tickets)
            {
                t.IsPaid = true;
                t.PaymentMethod = "Cash";
                if (t.Seat != null) t.Seat.IsBooked = true;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        public class CashPaymentRequest
        {
            public string OrderId { get; set; }
        }

        public class ComboRequest
        {
            public string OrderId { get; set; }
            public Dictionary<int, int> SelectedCombos { get; set; }
            public decimal ComboTotal { get; set; }
        }
    }

    public class BookingRequest
    {
        public int ShowtimeId { get; set; }
        public List<int> SeatIds { get; set; }
    }

    public class PaymentRequest
    {
        public string OrderId { get; set; }
        public long Amount { get; set; }
    }
}
