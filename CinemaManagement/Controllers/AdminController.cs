using CinemaManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult RevenueStats()
    {
        // Default: last 30 days
        var vm = new RevenueStatsViewModel
        {
            FromDate = DateTime.UtcNow.AddDays(-30),
            ToDate = DateTime.UtcNow,
            PaymentMethod = "All"
        };

        vm = GetRevenueStats(vm.FromDate.Value, vm.ToDate.Value, vm.PaymentMethod);

        return View(vm);
    }

    [HttpPost]
    public IActionResult RevenueStats(DateTime? fromDate, DateTime? toDate, string paymentMethod)
    {
        if (!fromDate.HasValue) fromDate = DateTime.UtcNow.AddMonths(-1);
        if (!toDate.HasValue) toDate = DateTime.UtcNow;
        if (string.IsNullOrEmpty(paymentMethod)) paymentMethod = "All";

        var vm = GetRevenueStats(fromDate.Value, toDate.Value, paymentMethod);

        return View(vm);
    }

    private RevenueStatsViewModel GetRevenueStats(DateTime from, DateTime to, string paymentMethod)
    {
        var ticketsQuery = _context.Tickets
            .Include(t => t.Showtime)
                .ThenInclude(s => s.Movie)
            .Include(t => t.Seat)
                .ThenInclude(s => s.Room)
                    .ThenInclude(r => r.Theater)
            .Where(t => t.IsPaid && t.BookingTime >= from && t.BookingTime <= to);

        if (paymentMethod == "Momo")
        {
            ticketsQuery = ticketsQuery.Where(t => t.PaymentMethod == "Momo");
        }
        else if (paymentMethod == "Cash")
        {
            ticketsQuery = ticketsQuery.Where(t => t.PaymentMethod == "Cash");
        }

        var tickets = ticketsQuery.ToList();

        var vm = new RevenueStatsViewModel
        {
            FromDate = from,
            ToDate = to,
            PaymentMethod = paymentMethod,
            TotalRevenue = tickets.Sum(t => t.Price),
            TotalTickets = tickets.Count,
            ByMovie = tickets
                .GroupBy(t => t.Showtime.Movie.Title)
                .Select(g => new RevenueByMovie
                {
                    MovieTitle = g.Key,
                    TicketsSold = g.Count(),
                    Revenue = g.Sum(t => t.Price)
                })
                .ToList(),
            ByTheater = tickets
                .GroupBy(t => t.Seat.Room.Theater.Name)
                .Select(g => new RevenueByTheater
                {
                    TheaterName = g.Key,
                    TicketsSold = g.Count(),
                    Revenue = g.Sum(t => t.Price)
                })
                .ToList(),
            ByShowtime = tickets
                .GroupBy(t => new { t.Showtime.ShowtimeId, t.Showtime.Movie.Title, t.Seat.Room.Theater.Name, t.Showtime.StartTime })
                .Select(g => new RevenueByShowtime
                {
                    MovieTitle = g.Key.Title,
                    TheaterName = g.Key.Name,
                    Showtime = g.Key.StartTime,
                    TicketsSold = g.Count(),
                    Revenue = g.Sum(t => t.Price)
                })
                .ToList()
        };

        return vm;
    }
}
