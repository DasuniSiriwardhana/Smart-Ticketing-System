using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTicketingSystem.Data;
using SmartTicketingSystem.Models;

namespace SmartTicketingSystem.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class ORGANIZER_UNITController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ORGANIZER_UNITController(ApplicationDbContext context)
        {
            _context = context;
        }

        // SEARCH
        public async Task<IActionResult> Search(
            string mode,
            int? organizerId,
            string unitType,
            string contactEmail,
            string contactPhone,
            char? status,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _context.Set<ORGANIZER_UNIT>().AsQueryable();

            if (mode == "OrganizerID" && organizerId.HasValue)
                query = query.Where(o => o.OrganizerID == organizerId.Value);

            else if (mode == "UnitType" && !string.IsNullOrWhiteSpace(unitType))
                query = query.Where(o => (o.UnitType ?? "").Contains(unitType));

            else if (mode == "ContactEmail" && !string.IsNullOrWhiteSpace(contactEmail))
                query = query.Where(o => (o.ContactEmail ?? "").Contains(contactEmail));

            else if (mode == "ContactPhone" && !string.IsNullOrWhiteSpace(contactPhone))
                query = query.Where(o => (o.ContactPhone ?? "").Contains(contactPhone));

            else if (mode == "Status" && status.HasValue)
                query = query.Where(o => o.status == status.Value);

            else if (mode == "DateRange")
            {
                if (fromDate.HasValue)
                    query = query.Where(o => o.CreatedAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(o => o.CreatedAt < toDate.Value.AddDays(1));
            }
            else if (mode == "Advanced")
            {
                if (organizerId.HasValue) query = query.Where(o => o.OrganizerID == organizerId.Value);
                if (!string.IsNullOrWhiteSpace(unitType)) query = query.Where(o => (o.UnitType ?? "").Contains(unitType));
                if (!string.IsNullOrWhiteSpace(contactEmail)) query = query.Where(o => (o.ContactEmail ?? "").Contains(contactEmail));
                if (!string.IsNullOrWhiteSpace(contactPhone)) query = query.Where(o => (o.ContactPhone ?? "").Contains(contactPhone));
                if (status.HasValue) query = query.Where(o => o.status == status.Value);
                if (fromDate.HasValue) query = query.Where(o => o.CreatedAt >= fromDate.Value);
                if (toDate.HasValue) query = query.Where(o => o.CreatedAt < toDate.Value.AddDays(1));
            }

            return View("Index", await query.ToListAsync());
        }

        // GET: ORGANIZER_UNIT
        public async Task<IActionResult> Index()
        {
            return View(await _context.ORGANIZER_UNIT.ToListAsync());
        }

        // GET: ORGANIZER_UNIT/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var unit = await _context.ORGANIZER_UNIT.FirstOrDefaultAsync(m => m.OrganizerID == id);
            if (unit == null) return NotFound();

            return View(unit);
        }

        // GET: ORGANIZER_UNIT/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ORGANIZER_UNIT/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrganizerID,UnitType,ContactEmail,ContactPhone,status")] ORGANIZER_UNIT unit)
        {
            if (!ModelState.IsValid)
                return View(unit);

            unit.CreatedAt = DateTime.Now;

            _context.Add(unit);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: ORGANIZER_UNIT/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var unit = await _context.ORGANIZER_UNIT.FindAsync(id);
            if (unit == null) return NotFound();

            return View(unit);
        }

        // POST: ORGANIZER_UNIT/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrganizerID,UnitType,ContactEmail,ContactPhone,status,CreatedAt")] ORGANIZER_UNIT unit)
        {
            if (id != unit.OrganizerID) return NotFound();

            if (!ModelState.IsValid)
                return View(unit);

            try
            {
                _context.Update(unit);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ORGANIZER_UNITExists(unit.OrganizerID)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: ORGANIZER_UNIT/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var unit = await _context.ORGANIZER_UNIT.FirstOrDefaultAsync(m => m.OrganizerID == id);
            if (unit == null) return NotFound();

            return View(unit);
        }

        // POST: ORGANIZER_UNIT/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var unit = await _context.ORGANIZER_UNIT.FindAsync(id);
            if (unit != null) _context.ORGANIZER_UNIT.Remove(unit);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ORGANIZER_UNITExists(int id)
        {
            return _context.ORGANIZER_UNIT.Any(e => e.OrganizerID == id);
        }
    }
}
