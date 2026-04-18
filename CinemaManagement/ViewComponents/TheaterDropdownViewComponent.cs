using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaManagement.Data;
using CinemaManagement.Models;
using System.Linq;
using CinemaManagement.ViewModels;

namespace CinemaManagement.ViewComponents
{
    public class TheaterDropdownViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public TheaterDropdownViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Lấy toàn bộ rạp, có City
            var theaters = await _context.Theaters
                .Where(t => !string.IsNullOrEmpty(t.City))
                .OrderBy(t => t.City)
                .ThenBy(t => t.Name)
                .ToListAsync();

            // Group theo City (string)
            var grouped = theaters
                .GroupBy(t => t.City)
                .Select(g => new TheaterCityGroupViewModel
                {
                    City = g.Key,
                    Theaters = g.ToList()
                })
                .ToList();

            return View(grouped);
        }
    }
}
