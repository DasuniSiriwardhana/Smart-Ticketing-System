using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTicketingSystem.Data;
using SmartTicketingSystem.Models;

namespace SmartTicketingSystem.Controllers
{
    public class WAITING_LISTController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WAITING_LISTController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Search Bar
        public async Task<IActionResult> Search(
            string mode,
            int? waitingListId,
            int? eventId,
            int? member_id,
            string status,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _context.Set<WAITING_LIST>().AsQueryable();

            if (mode == "WaitingListID" && waitingListId.HasValue)
                query = query.Where(w => w.WaitingListID == waitingListId.Value);

            else if (mode == "EventID" && eventId.HasValue)
                query = query.Where(w => w.EventID == eventId.Value);

            else if (mode == "MemberID" && member_id.HasValue)
                query = query.Where(w => w.member_id == member_id.Value);

            else if (mode == "Status" && !string.IsNullOrWhiteSpace(status))
                query = query.Where(w => (w.Status ?? "") == status);

            else if (mode == "DateRange")
            {
                if (fromDate.HasValue)
                    query = query.Where(w => w.AddedAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(w => w.AddedAt < toDate.Value.AddDays(1));
            }

            else if (mode == "Advanced")
            {
                if (waitingListId.HasValue)
                    query = query.Where(w => w.WaitingListID == waitingListId.Value);

                if (eventId.HasValue)
                    query = query.Where(w => w.EventID == eventId.Value);

                if (member_id.HasValue)
                    query = query.Where(w => w.member_id == member_id.Value);

                if (!string.IsNullOrWhiteSpace(status))
                    query = query.Where(w => (w.Status ?? "") == status);

                if (fromDate.HasValue)
                    query = query.Where(w => w.AddedAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(w => w.AddedAt < toDate.Value.AddDays(1));
            }

            return View("Index", await query
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync());
        }

        // GET: WAITING_LIST
        public async Task<IActionResult> Index()
        {
            return View(await _context.WAITING_LIST.ToListAsync());
        }

        // GET: WAITING_LIST/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var waiting = await _context.WAITING_LIST
                .FirstOrDefaultAsync(m => m.WaitingListID == id);

            if (waiting == null) return NotFound();

            return View(waiting);
        }

        // GET: WAITING_LIST/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: WAITING_LIST/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WaitingListID,EventID,member_id,AddedAt,Status")] WAITING_LIST waiting)
        {
            if (ModelState.IsValid)
            {
                _context.Add(waiting);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(waiting);
        }

        // GET: WAITING_LIST/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var waiting = await _context.WAITING_LIST.FindAsync(id);
            if (waiting == null) return NotFound();

            return View(waiting);
        }

        // POST: WAITING_LIST/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WaitingListID,EventID,member_id,AddedAt,Status")] WAITING_LIST waiting)
        {
            if (id != waiting.WaitingListID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(waiting);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.WAITING_LIST.Any(e => e.WaitingListID == waiting.WaitingListID))
                        return NotFound();

                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(waiting);
        }

        // GET: WAITING_LIST/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var waiting = await _context.WAITING_LIST
                .FirstOrDefaultAsync(m => m.WaitingListID == id);

            if (waiting == null) return NotFound();

            return View(waiting);
        }

        // POST: WAITING_LIST/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var waiting = await _context.WAITING_LIST.FindAsync(id);
            if (waiting != null)
                _context.WAITING_LIST.Remove(waiting);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
