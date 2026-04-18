using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CinemaManagement.Data;
using CinemaManagement.Helpers;
using CinemaManagement.Models;
using CinemaManagement.ViewModel;
using CinemaManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.Controllers
{
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class ManagerController : Controller
    {
        private readonly AppDbContext _context;

        public ManagerController(AppDbContext context)
        {
            _context = context;
        }

        // ===================== HELPER =====================
        private async Task<string> SaveImageAsync(IFormFile file, string folder)
        {
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", folder);
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/{folder}/{fileName}";
        }

        // ===================== DROPDOWN =====================
        private void SetDropdownData()
        {
            ViewBag.Genres = _context.Genres.ToList();
            ViewBag.Rooms = _context.Rooms.Include(r => r.Theater).ToList();
        }

        // ===================== INDEX =====================
        public async Task<IActionResult> Index(int? roomId, string search)
        {
            SetDropdownData();

            var movies = _context.Movies
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .Include(m => m.Showtimes).ThenInclude(s => s.Room).ThenInclude(r => r.Theater)
                .AsQueryable();

            if (roomId.HasValue)
                movies = movies.Where(m => m.Showtimes.Any(s => s.RoomId == roomId));

            if (!string.IsNullOrWhiteSpace(search))
                movies = movies.Where(m => m.Title.Contains(search));

            var vm = new TheaterMoviesViewModel
            {
                Movies = await movies.ToListAsync(),
                RoomId = roomId,
                RoomName = roomId.HasValue
                    ? (await _context.Rooms.Include(r => r.Theater)
                        .FirstOrDefaultAsync(r => r.RoomId == roomId))?.Name
                    : null,
                SearchQuery = search,
                IsFiltered = roomId.HasValue || !string.IsNullOrWhiteSpace(search),
                Rooms = await _context.Rooms.Include(r => r.Theater).ToListAsync()
            };

            return View(vm);
        }

        // ===================== CREATE =====================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            SetDropdownData();

            return View(new MovieFormViewModel
            {
                Movie = new Movie { ReleaseDate = DateTime.Today },
                Genres = await _context.Genres.ToListAsync(),
                SelectedGenres = new List<int>()
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(MovieFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Genres = await _context.Genres.ToListAsync();
                return View(vm);
            }

            // Upload Poster 3:4
            if (vm.PosterFile != null)
                vm.Movie.PosterUrl = await SaveImageAsync(vm.PosterFile, "posters");

            // Upload Banner 16:9
            if (vm.BannerFile != null)
                vm.Movie.BannerUrl = await SaveImageAsync(vm.BannerFile, "banners");
            // Youtube URL
            vm.Movie.VideoUrl = YoutubeHelper.ToEmbedUrl(vm.Movie.VideoUrl);
            _context.Movies.Add(vm.Movie);
            await _context.SaveChangesAsync();

            // Genres
            foreach (var gid in vm.SelectedGenres.Distinct())
            {
                _context.MovieGenres.Add(new MovieGenre
                {
                    MovieId = vm.Movie.MovieId,
                    GenreId = gid
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ===================== EDIT =====================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            SetDropdownData();

            var movie = await _context.Movies
                .Include(m => m.MovieGenres)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie == null) return NotFound();

            return View(new MovieFormViewModel
            {
                Movie = movie,
                Genres = await _context.Genres.ToListAsync(),
                SelectedGenres = movie.MovieGenres.Select(mg => mg.GenreId).ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, MovieFormViewModel vm)
        {
            if (id != vm.Movie.MovieId) return NotFound();

            if (!ModelState.IsValid)
            {
                vm.Genres = await _context.Genres.ToListAsync();
                return View(vm);
            }

            var movie = await _context.Movies
                .Include(m => m.MovieGenres)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie == null) return NotFound();

            movie.Title = vm.Movie.Title;
            movie.Description = vm.Movie.Description;
            movie.Duration = vm.Movie.Duration;
            movie.VideoUrl = vm.Movie.VideoUrl;
            movie.ReleaseDate = vm.Movie.ReleaseDate;

            // Poster
            if (vm.PosterFile != null)
            {
                movie.PosterUrl = await SaveImageAsync(vm.PosterFile, "posters");
            }
            else if (!string.IsNullOrWhiteSpace(vm.Movie.PosterUrl))
            {
                movie.PosterUrl = vm.Movie.PosterUrl;
            }

            // Banner
            if (vm.BannerFile != null)
            {
                movie.BannerUrl = await SaveImageAsync(vm.BannerFile, "banners");
            }
            else if (!string.IsNullOrWhiteSpace(vm.Movie.BannerUrl))
            {
                movie.BannerUrl = vm.Movie.BannerUrl;
            }

            // Youtube URL
            movie.VideoUrl = YoutubeHelper.ToEmbedUrl(vm.Movie.VideoUrl);
            _context.MovieGenres.RemoveRange(movie.MovieGenres);
            foreach (var gid in vm.SelectedGenres.Distinct())
            {
                _context.MovieGenres.Add(new MovieGenre
                {
                    MovieId = id,
                    GenreId = gid
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ===================== DETAILS =====================
        public async Task<IActionResult> Details(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .Include(m => m.Showtimes).ThenInclude(s => s.Room).ThenInclude(r => r.Theater)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie == null) return NotFound();
            return View(movie);
        }

        // ===================== Hide =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Hide(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            movie.IsActive = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Show(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            movie.IsActive = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
