using Microsoft.AspNetCore.Mvc;
using System.Linq;
using CinemaManagement.Data;
using CinemaManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.Controllers
{
    public class GenreController : Controller
    {
        private readonly AppDbContext _context;

        public GenreController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Hiển thị danh sách thể loại
        public IActionResult Index()
        {
            var genres = _context.Genres.ToList();
            return View(genres);
        }

        // GET: Thêm thể loại
        public IActionResult Create()
        {
            return View();
        }

        // POST: Thêm thể loại
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Genre genre)
        {
            if (!ModelState.IsValid) return View(genre);

            _context.Genres.Add(genre);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // GET: Sửa thể loại
        public IActionResult Edit(int id)
        {
            var genre = _context.Genres.Find(id);
            if (genre == null) return NotFound();

            return View(genre);
        }

        // POST: Sửa thể loại
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Genre genre)
        {
            if (id != genre.GenreId) return NotFound();

            if (!ModelState.IsValid) return View(genre);

            _context.Update(genre);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // POST: Xoá thể loại từ popup trong Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int genreId)
        {
            var genre = _context.Genres
                .Include(g => g.MovieGenres)
                .FirstOrDefault(g => g.GenreId == genreId);

            if (genre == null) return NotFound();

            if (genre.MovieGenres != null && genre.MovieGenres.Any())
            {
                TempData["Error"] = "Không thể xóa thể loại đang được sử dụng bởi phim.";
                return RedirectToAction(nameof(Index));
            }

            _context.Genres.Remove(genre);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // ✅ Lọc phim theo thể loại
        [HttpPost]
        public IActionResult Filter(int[] selectedGenres)
        {
            if (selectedGenres == null || selectedGenres.Length == 0)
            {
                TempData["Error"] = "⛔ Vui lòng chọn ít nhất một thể loại!";
                return RedirectToAction("Index");
            }

            var movies = _context.Movies
                .Include(m => m.MovieGenres)
                    .ThenInclude(mg => mg.Genre)
                .Where(m => m.MovieGenres.Any(mg => selectedGenres.Contains(mg.GenreId)))
                .ToList();

            return View("FilteredMovies", movies);
        }
    }
}
