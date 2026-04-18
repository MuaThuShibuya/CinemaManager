using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using CinemaManagement.Models;

public class SeatResetService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    // Thời gian reserve tối đa (phút)
    private const int ReservationMinutes = 1;

    public SeatResetService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var now = DateTime.UtcNow;

                // 1️⃣ Reset ghế chưa thanh toán > ReservationMinutes
                var expiredTickets = await context.Tickets
                    .Where(t => !t.IsPaid && t.BookingTime < now.AddMinutes(-ReservationMinutes))
                    .ToListAsync(stoppingToken);

                foreach (var ticket in expiredTickets)
                {
                    var seat = await context.Seats.FirstOrDefaultAsync(s => s.SeatId == ticket.SeatId, stoppingToken);
                    if (seat != null)
                        seat.IsBooked = false;

                    context.Tickets.Remove(ticket); // xóa ticket chưa thanh toán
                }

                // 2️⃣ Reset ghế của các suất chiếu đã kết thúc
                var endedShowtimes = await context.Showtimes
                    .Include(s => s.Movie)
                    .Where(s => s.StartTime.AddMinutes(s.Movie.Duration) < now)
                    .ToListAsync(stoppingToken);

                foreach (var showtime in endedShowtimes)
                {
                    var tickets = await context.Tickets
                        .Where(t => t.ShowtimeId == showtime.ShowtimeId && t.IsPaid)
                        .ToListAsync(stoppingToken);

                    foreach (var ticket in tickets)
                    {
                        var seat = await context.Seats.FirstOrDefaultAsync(s => s.SeatId == ticket.SeatId, stoppingToken);
                        if (seat != null)
                            seat.IsBooked = false;
                    }
                }

                await context.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                // Bạn có thể log lỗi ở đây nếu muốn
                Console.WriteLine($"SeatResetService error: {ex.Message}");
            }

            // Chạy lại sau mỗi 1 phút
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
