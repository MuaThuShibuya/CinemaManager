using CinemaManagement.Data;
using CinemaManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CinemaManagement.ViewModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using CinemaManagement.ViewModels;

namespace CinemaManagement.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class TheaterController : Controller
    {
        private readonly AppDbContext _context;

        public TheaterController(AppDbContext context)
        {
            _context = context;
        }

        // Danh sách tất cả rạp
        public IActionResult Index()
        {
            var theaters = _context.Theaters
                .Include(t => t.Rooms)
                .ToList();

            return View(theaters);
        }

        // Xem chi tiết rạp
        public IActionResult Details(int id)
        {
            var theater = _context.Theaters
                .Include(t => t.Rooms)
                    .ThenInclude(r => r.Seats)
                .FirstOrDefault(t => t.TheaterId == id);

            if (theater == null) return NotFound();
            return View(theater);
        }

        // Form thêm rạp
        public IActionResult Create()
        {
            return View(new TheaterFormViewModel { Theater = new Theater() });
        }

        [HttpPost]
        public IActionResult Create(TheaterFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.Theaters.Add(model.Theater);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // Form sửa rạp
        public async Task<IActionResult> Edit(int id)
        {
            var theater = await _context.Theaters.FirstOrDefaultAsync(t => t.TheaterId == id);
            if (theater == null)
            {
                return NotFound();
            }
            var vm = new TheaterFormViewModel { Theater = theater };
            return View(vm);
        }

        // Xử lý sửa rạp
        [HttpPost]
        public IActionResult Edit(int id, TheaterFormViewModel model)
        {
            if (id != model.Theater.TheaterId) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var theater = _context.Theaters.FirstOrDefault(t => t.TheaterId == id);
            if (theater == null) return NotFound();

            theater.Name = model.Theater.Name;
            theater.City = model.Theater.City;
            theater.Location = model.Theater.Location;

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // Vô hiệu hóa rạp (GET)
        public IActionResult Disable(int id)
        {
            var theater = _context.Theaters.FirstOrDefault(t => t.TheaterId == id);
            if (theater == null) return NotFound();

            return View("Disable", theater);
        }

        // Kích hoạt lại rạp (GET)
        public IActionResult Enable(int id)
        {
            var theater = _context.Theaters.FirstOrDefault(t => t.TheaterId == id);
            if (theater == null) return NotFound();

            return View("Enable", theater);
        }

        // Xử lý vô hiệu hóa rạp (POST)
        [HttpPost, ActionName("Disable")]
        public IActionResult DisableConfirmed(int id)
        {
            var theater = _context.Theaters.FirstOrDefault(t => t.TheaterId == id);
            if (theater == null)
                return NotFound();

            theater.IsActive = false;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "✅ Rạp đã được vô hiệu hóa thành công.";
            return RedirectToAction(nameof(Index));
        }

        // Xử lý kích hoạt lại rạp (POST)
        [HttpPost, ActionName("Enable")]
        public IActionResult EnableConfirmed(int id)
        {
            var theater = _context.Theaters.FirstOrDefault(t => t.TheaterId == id);
            if (theater == null)
                return NotFound();

            theater.IsActive = true;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "✅ Rạp đã được kích hoạt lại thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult CreateRoom(int theaterId)
        {
            var theater = _context.Theaters.FirstOrDefault(t => t.TheaterId == theaterId);
            if (theater == null)
                return NotFound();

            var model = new RoomFormViewModel
            {
                TheaterId = theaterId
            };

            ViewBag.TheaterName = theater.Name;
            ViewBag.TheaterCity = theater.City;

            return View(model);
        }

        [HttpPost]
        public IActionResult CreateRoom(RoomFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var theater = _context.Theaters.FirstOrDefault(t => t.TheaterId == model.TheaterId);
                ViewBag.TheaterName = theater?.Name ?? "";
                ViewBag.TheaterCity = theater?.City ?? "";
                return View(model);
            }

            var room = new Room
            {
                TheaterId = model.TheaterId,
                Name = model.RoomName,
                PriceMultiplier = model.PriceMultiplier ?? 1.0m,
                Description = model.Description
            };

            _context.Rooms.Add(room);
            _context.SaveChanges();

            for (int i = 1; i <= model.SeatCount; i++)
            {
                var seat = new Seat
                {
                    RoomId = room.RoomId,
                    SeatNumber = i,
                    IsBooked = false
                };
                _context.Seats.Add(seat);
            }
            _context.SaveChanges();

            return RedirectToAction("Details", new { id = model.TheaterId });
        }

        // ==========================================
        // THÊM MỚI: Xử lý vô hiệu hóa Phòng chiếu
        // ==========================================
        [HttpPost]
        public IActionResult DisableRoom(int id)
        {
            var room = _context.Rooms.FirstOrDefault(r => r.RoomId == id);
            if (room == null) return NotFound();

            int theaterId = room.TheaterId;
            room.IsActive = false;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "✅ Phòng đã được vô hiệu hóa thành công.";
            return RedirectToAction(nameof(Details), new { id = theaterId });
        }

        // Kích hoạt lại phòng chiếu
        [HttpPost]
        public IActionResult EnableRoom(int id)
        {
            var room = _context.Rooms.FirstOrDefault(r => r.RoomId == id);
            if (room == null) return NotFound();

            int theaterId = room.TheaterId;
            room.IsActive = true;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "✅ Phòng đã được kích hoạt lại thành công.";
            return RedirectToAction(nameof(Details), new { id = theaterId });
        }
    }
}