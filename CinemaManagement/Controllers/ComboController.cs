using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaManagement.Data;
using CinemaManagement.Models;

namespace CinemaManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ComboController : Controller
    {
        private readonly AppDbContext _context;

        public ComboController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Combo
        public async Task<IActionResult> Index()
        {
            return View(await _context.Combos.ToListAsync());
        }

        // GET: Combo/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Combo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Price,ImageUrl,IsAvailable")] Combo combo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(combo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(combo);
        }

        // GET: Combo/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var combo = await _context.Combos.FindAsync(id);
            if (combo == null)
            {
                return NotFound();
            }
            return View(combo);
        }

        // POST: Combo/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ComboId,Name,Description,Price,ImageUrl,IsAvailable")] Combo combo)
        {
            if (id != combo.ComboId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(combo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ComboExists(combo.ComboId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(combo);
        }

        // GET: Combo/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var combo = await _context.Combos
                .FirstOrDefaultAsync(m => m.ComboId == id);
            if (combo == null)
            {
                return NotFound();
            }

            return View(combo);
        }

        // POST: Combo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var combo = await _context.Combos.FindAsync(id);
            if (combo != null)
            {
                _context.Combos.Remove(combo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ComboExists(int id)
        {
            return _context.Combos.Any(e => e.ComboId == id);
        }
    }
}