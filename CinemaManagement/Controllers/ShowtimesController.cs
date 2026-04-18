using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaManagement.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CinemaManagement.Controllers
{
    public class ShowtimesController : Controller
    {
        private readonly AppDbContext _context;

        // Thời gian dọn phòng sau mỗi suất chiếu (phút)
        private const int CleanUpMinutes = 15;

        public ShowtimesController(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // =========================
        // CHECK TRÙNG SUẤT CHIẾU
        // =========================
        private bool CheckShowtimeConflict(
            int roomId,
            DateTime startTime,
            int movieId,
            int? showtimeIdToExclude = null)
        {
            var movie = _context.Movies.FirstOrDefault(m => m.MovieId == movieId);
            if (movie == null || movie.Duration <= 0)
                return false;

            var newEndTime = startTime.AddMinutes(movie.Duration + CleanUpMinutes);

            var query = _context.Showtimes
                .Include(s => s.Movie)
                .Where(s =>
                    s.RoomId == roomId &&
                    s.StartTime.Date == startTime.Date);

            if (showtimeIdToExclude.HasValue)
            {
                query = query.Where(s => s.ShowtimeId != showtimeIdToExclude.Value);
            }

            var existingShowtimes = query.ToList();

            foreach (var s in existingShowtimes)
            {
                if (s.Movie == null || s.Movie.Duration <= 0)
                    continue;

                var existingEndTime =
                    s.StartTime.AddMinutes(s.Movie.Duration + CleanUpMinutes);

                // Check overlap
                if (startTime < existingEndTime && newEndTime > s.StartTime)
                {
                    return true;
                }
            }

            return false;
        }

        // =========================
        // INDEX
        // =========================
        public IActionResult Index(int? movieId)
        {
            var showtimes = _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Room)
                    .ThenInclude(r => r.Theater)
                .AsQueryable();

            if (movieId.HasValue)
            {
                ViewBag.MovieId = movieId.Value;
                ViewBag.MovieTitle = _context.Movies
                    .FirstOrDefault(m => m.MovieId == movieId.Value)?.Title;

                showtimes = showtimes.Where(s => s.MovieId == movieId.Value);
            }

            return View(showtimes.ToList());
        }

        // =========================
        // DETAILS
        // =========================
        public IActionResult Details(int id)
        {
            var showtime = _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Room)
                    .ThenInclude(r => r.Theater)
                .FirstOrDefault(s => s.ShowtimeId == id);

            if (showtime == null)
                return NotFound();

            return View(showtime);
        }

        // =========================
        // CREATE (GET)
        // =========================
        [HttpGet]
        public IActionResult Create(int movieId)
        {
            var movie = _context.Movies.FirstOrDefault(m => m.MovieId == movieId);
            if (movie == null)
                return NotFound();

            ViewBag.MovieId = movieId;
            ViewBag.MovieTitle = movie.Title;
            ViewBag.Theaters = _context.Theaters.Where(t => t.IsActive).ToList();
            ViewBag.Rooms = _context.Rooms.Where(r => r.IsActive).ToList();

            return View(new ShowtimeViewModel
            {
                MovieId = movieId
            });
        }

        // =========================
        // CREATE (POST)
        // =========================
        [HttpPost]
        public IActionResult Create(ShowtimeViewModel model)
        {
            ViewBag.Theaters = _context.Theaters.ToList();
            ViewBag.Rooms = _context.Rooms.ToList();
            ViewBag.MovieTitle = _context.Movies
                .FirstOrDefault(m => m.MovieId == model.MovieId)?.Title;

            if (!ModelState.IsValid)
                return View(model);

            if (CheckShowtimeConflict(model.RoomId, model.StartTime, model.MovieId))
            {
                ModelState.AddModelError("",
                    "❌ Suất chiếu bị trùng hoặc chưa đủ thời gian dọn phòng.");
                return View(model);
            }

            var movie = _context.Movies.FirstOrDefault(m => m.MovieId == model.MovieId);
            var calculatedEndTime =
                model.StartTime.AddMinutes(movie.Duration + CleanUpMinutes);

            var showtime = new Showtime
            {
                MovieId = model.MovieId,
                RoomId = model.RoomId,
                StartTime = model.StartTime,
                EndTime = calculatedEndTime,
                Price = model.Price,

                DiscountPercent = model.DiscountPercent,
                DiscountStart = model.DiscountStart,
                DiscountEnd = model.DiscountEnd
            };

            _context.Showtimes.Add(showtime);
            _context.SaveChanges();

            TempData["Success"] = "✅ Đã thêm suất chiếu mới!";
            return RedirectToAction("Index", new { movieId = model.MovieId });
        }

        // =========================
        // EDIT (GET)
        // =========================
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var showtime = _context.Showtimes
                .Include(s => s.Room)
                    .ThenInclude(r => r.Theater)
                .FirstOrDefault(s => s.ShowtimeId == id);

            if (showtime == null)
                return NotFound();

            var vm = new ShowtimeViewModel
            {
                ShowtimeId = showtime.ShowtimeId,
                MovieId = showtime.MovieId,
                RoomId = showtime.RoomId,
                TheaterId = showtime.Room?.TheaterId ?? 0,
                StartTime = showtime.StartTime,
                EndTime = showtime.EndTime,
                Price = showtime.Price,

                DiscountPercent = showtime.DiscountPercent,
                DiscountStart = showtime.DiscountStart,
                DiscountEnd = showtime.DiscountEnd
            };

            ViewBag.MovieId = vm.MovieId;
            ViewBag.MovieTitle = _context.Movies
                .FirstOrDefault(m => m.MovieId == vm.MovieId)?.Title;

            ViewBag.TheaterList = new SelectList(
                _context.Theaters.Where(t => t.IsActive).ToList(),
                "TheaterId",
                "Name",
                vm.TheaterId
            );

            ViewBag.RoomList = new SelectList(
                _context.Rooms.Where(r => r.TheaterId == vm.TheaterId && r.IsActive).ToList(),
                "RoomId",
                "Name",
                vm.RoomId
            );

            return View(vm);
        }

        // =========================
        // EDIT (POST)
        // =========================
        [HttpPost]
        public IActionResult Edit(ShowtimeViewModel vm)
        {
            ViewBag.MovieId = vm.MovieId;
            ViewBag.MovieTitle = _context.Movies
                .FirstOrDefault(m => m.MovieId == vm.MovieId)?.Title;

            ViewBag.TheaterList = new SelectList(
                _context.Theaters.Where(t => t.IsActive).ToList(),
                "TheaterId",
                "Name",
                vm.TheaterId
            );

            ViewBag.RoomList = new SelectList(
                _context.Rooms.Where(r => r.TheaterId == vm.TheaterId && r.IsActive).ToList(),
                "RoomId",
                "Name",
                vm.RoomId
            );

            if (!ModelState.IsValid)
                return View(vm);

            if (CheckShowtimeConflict(vm.RoomId, vm.StartTime, vm.MovieId, vm.ShowtimeId))
            {
                ModelState.AddModelError("",
                    "❌ Suất chiếu bị trùng hoặc chưa đủ thời gian dọn phòng.");
                return View(vm);
            }

            var showtime = _context.Showtimes
                .FirstOrDefault(s => s.ShowtimeId == vm.ShowtimeId);

            if (showtime == null)
                return NotFound();

            var movie = _context.Movies.FirstOrDefault(m => m.MovieId == vm.MovieId);
            var calculatedEndTime =
                vm.StartTime.AddMinutes(movie.Duration + CleanUpMinutes);

            showtime.MovieId = vm.MovieId;
            showtime.RoomId = vm.RoomId;
            showtime.StartTime = vm.StartTime;
            showtime.EndTime = calculatedEndTime;
            showtime.Price = vm.Price;

            showtime.DiscountPercent = vm.DiscountPercent;
            showtime.DiscountStart = vm.DiscountStart;
            showtime.DiscountEnd = vm.DiscountEnd;

            _context.SaveChanges();

            TempData["Success"] = "✅ Cập nhật suất chiếu thành công!";
            return RedirectToAction("Index", new { movieId = vm.MovieId });
        }

        // =========================
        // DELETE
        // =========================
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var showtime = _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Room)
                    .ThenInclude(r => r.Theater)
                .FirstOrDefault(s => s.ShowtimeId == id);

            if (showtime == null)
                return NotFound();

            return View(showtime);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var showtime = await _context.Showtimes.FindAsync(id);
            if (showtime != null)
            {
                _context.Showtimes.Remove(showtime);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // AJAX: LOAD PHÒNG THEO RẠP
        // =========================
        [HttpGet]
        public IActionResult GetRoomsByTheater(int theaterId)
        {
            var rooms = _context.Rooms
                .Where(r => r.TheaterId == theaterId && r.IsActive)
                .Select(r => new
                {
                    roomId = r.RoomId,
                    name = r.Name
                })
                .ToList();

            return Json(rooms);
        }
    }
}
