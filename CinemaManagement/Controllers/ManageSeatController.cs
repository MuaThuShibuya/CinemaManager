using CinemaManagement.Data;
using CinemaManagement.Models;
using CinemaManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.Controllers
{
    public class ManageSeatController : Controller
    {
        private readonly AppDbContext _context;

        public ManageSeatController(AppDbContext context)
        {
            _context = context;
        }

        // ==================== LOAD EDIT ROOM ====================
        public async Task<IActionResult> EditRoom(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.Seats)
                .FirstOrDefaultAsync(r => r.RoomId == id);

            if (room == null) return NotFound();

            string[] required = { "Regular", "Vip", "Couple" };

            var prices = await _context.RoomSeatTypePrices
                .Where(p => p.RoomId == id)
                .ToListAsync();

            foreach (var t in required)
            {
                if (!prices.Any(p => p.SeatType == t))
                {
                    _context.RoomSeatTypePrices.Add(new RoomSeatTypePrice
                    {
                        RoomId = id,
                        SeatType = t,
                        Price = 0
                    });
                }
            }
            await _context.SaveChangesAsync();

            prices = await _context.RoomSeatTypePrices
                .Where(p => p.RoomId == id)
                .ToListAsync();

            var vm = new EditRoomViewModel
            {
                RoomId = room.RoomId,
                TheaterId = room.TheaterId,
                RoomName = room.Name,
                Description = room.Description,
                PriceMultiplier = room.PriceMultiplier,
                Seats = room.Seats.OrderBy(s => s.SeatNumber).ToList(),
                SeatTypePrices = prices
            };

            return View(vm);
        }

        // ==================== SAVE BASIC ROOM INFO ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoom(EditRoomViewModel model)
        {
            var room = await _context.Rooms.FindAsync(model.RoomId);
            if (room == null) return NotFound();

            room.Name = model.RoomName;
            room.Description = model.Description;
            room.PriceMultiplier = model.PriceMultiplier;

            // update seat type prices
            foreach (var item in model.SeatTypePrices)
            {
                var db = await _context.RoomSeatTypePrices
                    .FirstOrDefaultAsync(p => p.RoomId == model.RoomId && p.SeatType == item.SeatType);

                if (db == null)
                {
                    _context.RoomSeatTypePrices.Add(new RoomSeatTypePrice
                    {
                        RoomId = model.RoomId,
                        SeatType = item.SeatType,
                        Price = item.Price
                    });
                }
                else
                {
                    db.Price = item.Price;
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Theater", new { id = room.TheaterId });
        }

        // ==================== AJAX: UPDATE MANY SEAT TYPES ====================
        [HttpPost]
        public async Task<IActionResult> UpdateSeatType([FromBody] UpdateSeatTypeDto dto)
        {
            if (dto == null || dto.SeatIds == null || dto.SeatIds.Count == 0)
                return Json(new { success = false, error = "No seats" });

            // Load từng seat 1 → EF không bao giờ sinh WITH
            foreach (var seatId in dto.SeatIds)
            {
                var seat = await _context.Seats.FindAsync(seatId);
                if (seat == null) continue;

                if (Enum.TryParse<SeatType>(dto.SeatType, out var st))
                {
                    seat.SeatType = st;
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // ==================== AJAX: UPDATE PRICE TABLE ====================
        [HttpPost]
        public async Task<IActionResult> UpdatePrices([FromBody] UpdatePricesDto dto)
        {
            var list = await _context.RoomSeatTypePrices
                .Where(p => p.RoomId == dto.RoomId)
                .ToListAsync();

            void UpdateOne(string type, decimal price)
            {
                var rec = list.FirstOrDefault(x => x.SeatType == type);
                if (rec != null)
                    rec.Price = price;
            }

            UpdateOne("Regular", dto.Regular);
            UpdateOne("Vip", dto.Vip);
            UpdateOne("Couple", dto.Couple);

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }

    // DTOs
    public class UpdateSeatTypeDto
    {
        public List<int> SeatIds { get; set; }
        public string SeatType { get; set; }
    }

    public class UpdatePricesDto
    {
        public int RoomId { get; set; }
        public decimal Regular { get; set; }
        public decimal Vip { get; set; }
        public decimal Couple { get; set; }
    }
}
