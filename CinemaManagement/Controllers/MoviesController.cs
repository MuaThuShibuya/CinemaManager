using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CinemaManagement.Models;
using CinemaManagement.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.Controllers
{
    public class MoviesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MoviesController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /* ===================== COMMON DATA ===================== */
        protected void SetCommonData()
        {
            ViewBag.Theaters = _context.Theaters.ToList();
            ViewBag.Genres = _context.Genres.ToList();
            ViewBag.ShowtimeHours = _context.Showtimes
                .Select(s => s.StartTime.Hour)
                .Distinct()
                .OrderBy(h => h)
                .ToList();
        }

        /* ===================== HOME ===================== */
        public async Task<IActionResult> Index()
        {
            SetCommonData();

            var now = DateTime.Now;
            var firstDayOfCurrentMonth = new DateTime(now.Year, now.Month, 1);
            var firstDayOfNextMonth = firstDayOfCurrentMonth.AddMonths(1);

            ViewBag.TopRatedMovies = await _context.Movies
                .Where(m => m.IsActive)
                .Include(m => m.MovieGenres)
                    .ThenInclude(mg => mg.Genre)
                .Include(m => m.Showtimes)
                .Where(m => m.Showtimes.Any(s =>
                    s.StartTime >= firstDayOfCurrentMonth &&
                    s.StartTime < firstDayOfNextMonth))
                .OrderByDescending(m => m.Rating)
                .Take(10)
                .ToListAsync();

            return View();
        }

        /* ===================== NOW SHOWING ===================== */
        public async Task<IActionResult> NowShowing(string searchQuery, int? theaterId, string sortBy, int? genreId)
        {
            SetCommonData();

            var now = DateTime.Now;
            var firstDay = new DateTime(now.Year, now.Month, 1);
            var nextMonth = firstDay.AddMonths(1);

            var movies = _context.Movies
                .Where(m => m.IsActive)
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .Include(m => m.Showtimes)
                    .ThenInclude(s => s.Room)
                        .ThenInclude(r => r.Theater)
                .Where(m => m.Showtimes.Any(s => s.StartTime >= firstDay && s.StartTime < nextMonth))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
                movies = movies.Where(m => m.Title.Contains(searchQuery));

            if (genreId.HasValue)
                movies = movies.Where(m => m.MovieGenres.Any(mg => mg.GenreId == genreId));

            if (theaterId.HasValue)
                movies = movies.Where(m => m.Showtimes.Any(s => s.Room.TheaterId == theaterId));

            movies = sortBy switch
            {
                "title_asc" => movies.OrderBy(m => m.Title),
                "title_desc" => movies.OrderByDescending(m => m.Title),
                "rating_asc" => movies.OrderBy(m => m.Rating),
                "rating_desc" => movies.OrderByDescending(m => m.Rating),
                _ => movies
            };

            var model = new TheaterMoviesViewModel
            {
                Movies = await movies.ToListAsync(),
                IsFiltered = theaterId.HasValue,
                TheaterName = theaterId.HasValue
                    ? await _context.Theaters.Where(t => t.TheaterId == theaterId)
                        .Select(t => t.Name).FirstOrDefaultAsync()
                    : null
            };

            return View("MovieList", model);
        }

        /* ===================== COMING SOON ===================== */
        public async Task<IActionResult> ComingSoon(string searchQuery, int? theaterId, string sortBy, int? genreId)
        {
            SetCommonData();

            var now = DateTime.Now;
            var firstDayNextMonth = new DateTime(now.Year, now.Month, 1).AddMonths(1);

            var movies = _context.Movies
                .Where(m => m.IsActive)
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .Include(m => m.Showtimes)
                    .ThenInclude(s => s.Room)
                        .ThenInclude(r => r.Theater)
                .Where(m => m.Showtimes.Any(s => s.StartTime >= firstDayNextMonth))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
                movies = movies.Where(m => m.Title.Contains(searchQuery));

            if (genreId.HasValue)
                movies = movies.Where(m => m.MovieGenres.Any(mg => mg.GenreId == genreId));

            if (theaterId.HasValue)
                movies = movies.Where(m => m.Showtimes.Any(s => s.Room.TheaterId == theaterId));

            movies = sortBy switch
            {
                "title_asc" => movies.OrderBy(m => m.Title),
                "title_desc" => movies.OrderByDescending(m => m.Title),
                "rating_asc" => movies.OrderBy(m => m.Rating),
                "rating_desc" => movies.OrderByDescending(m => m.Rating),
                _ => movies
            };

            var model = new TheaterMoviesViewModel
            {
                Movies = await movies.ToListAsync(),
                IsFiltered = theaterId.HasValue,
                TheaterName = theaterId.HasValue
                    ? await _context.Theaters.Where(t => t.TheaterId == theaterId)
                        .Select(t => t.Name).FirstOrDefaultAsync()
                    : null
            };

            return View("MovieList", model);
        }

        /* ===================== AJAX FILTER ===================== */
        [HttpGet]
        public IActionResult FilterAjax(string searchQuery, int? theaterId, int? genreId, int? time, string action)
        {
            var now = DateTime.Now;
            var firstDay = new DateTime(now.Year, now.Month, 1);
            var nextMonth = firstDay.AddMonths(1);

            var movies = _context.Movies
                .Where(m => m.IsActive) 
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .Include(m => m.Showtimes)
                    .ThenInclude(s => s.Room)
                        .ThenInclude(r => r.Theater)
                .AsQueryable();

            if (action == "NowShowing")
                movies = movies.Where(m => m.Showtimes.Any(s => s.StartTime >= firstDay && s.StartTime < nextMonth));
            else if (action == "ComingSoon")
                movies = movies.Where(m => m.Showtimes.Any(s => s.StartTime >= nextMonth));

            if (!string.IsNullOrWhiteSpace(searchQuery))
                movies = movies.Where(m => m.Title.Contains(searchQuery));

            if (genreId.HasValue)
                movies = movies.Where(m => m.MovieGenres.Any(mg => mg.GenreId == genreId));

            if (theaterId.HasValue)
                movies = movies.Where(m => m.Showtimes.Any(s => s.Room.TheaterId == theaterId));

            if (time.HasValue)
                movies = movies.Where(m => m.Showtimes.Any(s => s.StartTime.Hour == time));

            return PartialView("_MovieCardsPartial", movies.ToList());
        }

        /* ===================== DETAILS / WATCH ===================== */
        public IActionResult Details(int id)
        {
            var movie = _context.Movies
                .Where(m => m.IsActive)
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .Include(m => m.Showtimes)
                    .ThenInclude(s => s.Room)
                        .ThenInclude(r => r.Theater)
                .Include(m => m.MovieRatings)
                .FirstOrDefault(m => m.MovieId == id);

            if (movie == null)
            {
                TempData["Error"] = "❌ Phim không tồn tại!";
                return RedirectToAction("Index");
            }

            HttpContext.Session.SetInt32("MovieId", id);
            return View(movie);
        }

        public IActionResult Watch(int id)
        {
            var movie = _context.Movies.FirstOrDefault(m => m.MovieId == id);
            if (movie == null)
            {
                TempData["Error"] = "❌ Phim không tồn tại!";
                return RedirectToAction("Index");
            }

            ViewBag.MovieEmbedUrl = movie.VideoUrl;
            return View();
        }

        /* ===================== SHOWTIMES ===================== */
        public IActionResult Showtimes(int movieId, int? theaterId)
        {
            var showtimes = _context.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Room).ThenInclude(r => r.Theater)
                .Where(s => s.MovieId == movieId);

            if (theaterId.HasValue)
                showtimes = showtimes.Where(s => s.Room.TheaterId == theaterId);

            var result = showtimes.ToList();

            if (!result.Any())
            {
                TempData["Error"] = "⛔ Không có suất chiếu khả dụng.";
                return RedirectToAction("Index");
            }

            return View(result);
        }

        /* ===================== RATE MOVIE ===================== */
        [HttpPost]
        public IActionResult RateMovie(int movieId, int rating)
        {
            if (rating < 1 || rating > 10)
            {
                TempData["Error"] = "Đánh giá không hợp lệ.";
                return RedirectToAction("Details", new { id = movieId });
            }

            var movie = _context.Movies.FirstOrDefault(m => m.MovieId == movieId);
            var user = _userManager.GetUserAsync(User).Result;

            if (movie == null || user == null)
                return RedirectToAction("Details", new { id = movieId });

            var existing = _context.MovieRatings
                .FirstOrDefault(r => r.MovieId == movieId && r.UserId == user.Id);

            if (existing != null)
            {
                existing.Rating = rating;
                existing.RatedAt = DateTime.Now;
            }
            else
            {
                _context.MovieRatings.Add(new MovieRating
                {
                    MovieId = movieId,
                    UserId = user.Id,
                    Rating = rating,
                    RatedAt = DateTime.Now
                });
            }

            _context.SaveChanges();

            movie.Rating = Math.Round(
                _context.MovieRatings.Where(r => r.MovieId == movieId).Average(r => r.Rating), 1);

            _context.SaveChanges();

            TempData["Success"] = "Cảm ơn bạn đã đánh giá!";
            return RedirectToAction("Details", new { id = movieId });
        }

        /* ===================== FILTER SHOWTIMES (CITY + DATE) ===================== */
        [HttpGet]
        public IActionResult FilterShowtimes(int movieId, string date, string city)
        {
            string decodedCity = string.IsNullOrEmpty(city) ? null : WebUtility.UrlDecode(city);
            DateTime? selectedDate = DateTime.TryParse(date, out var d) ? d.Date : null;

            var showtimes = _context.Showtimes
                .Include(s => s.Room).ThenInclude(r => r.Theater)
                .Where(s => s.MovieId == movieId);

            if (selectedDate.HasValue)
                showtimes = showtimes.Where(s => s.StartTime.Date == selectedDate.Value);

            if (!string.IsNullOrEmpty(decodedCity))
            {
                var normalizedCity = decodedCity.Trim().ToLower();
                showtimes = showtimes.Where(s =>
                    s.Room.Theater.City != null &&
                    s.Room.Theater.City.Trim().ToLower() == normalizedCity);
            }

            return Json(showtimes
                .OrderBy(s => s.StartTime)
                .Select(s => new
                {
                    s.ShowtimeId,
                    City = s.Room.Theater.City,
                    Theater = s.Room.Theater.Name,
                    Room = s.Room.Name,
                    StartTime = s.StartTime.ToString("HH:mm"),
                    s.Price,
                    s.DiscountPercent
                }).ToList());
        }
    }
}
