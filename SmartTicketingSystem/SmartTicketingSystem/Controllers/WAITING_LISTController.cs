using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        //Search Bar
public async Task<IActionResult> Search(
    string mode,
    int? waitingListId,
    int? eventId,
    int? userId,
    string status,
    DateTime? fromDate,
    DateTime? toDate)
    {
        var query = _context.Set<WAITING_LIST>().AsQueryable();

        if (mode == "WaitingListID" && waitingListId.HasValue)
            query = query.Where(w => w.WaitingListID == waitingListId.Value);

        else if (mode == "EventID" && eventId.HasValue)
            query = query.Where(w => w.EventID == eventId.Value);

        else if (mode == "UserID" && userId.HasValue)
            query = query.Where(w => w.UserID == userId.Value);

        else if (mode == "Status" && !string.IsNullOrWhiteSpace(status))
            query = query.Where(w => w.Status == status);

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

            if (userId.HasValue)
                query = query.Where(w => w.UserID == userId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(w => w.Status == status);

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
            if (id == null)
            {
                return NotFound();
            }

            var wAITING_LIST = await _context.WAITING_LIST
                .FirstOrDefaultAsync(m => m.WaitingListID == id);
            if (wAITING_LIST == null)
            {
                return NotFound();
            }

            return View(wAITING_LIST);
        }

        // GET: WAITING_LIST/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: WAITING_LIST/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WaitingListID,EventID,UserID,AddedAt,Status")] WAITING_LIST wAITING_LIST)
        {
            if (ModelState.IsValid)
            {
                _context.Add(wAITING_LIST);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(wAITING_LIST);
        }

        // GET: WAITING_LIST/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var wAITING_LIST = await _context.WAITING_LIST.FindAsync(id);
            if (wAITING_LIST == null)
            {
                return NotFound();
            }
            return View(wAITING_LIST);
        }

        // POST: WAITING_LIST/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WaitingListID,EventID,UserID,AddedAt,Status")] WAITING_LIST wAITING_LIST)
        {
            if (id != wAITING_LIST.WaitingListID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(wAITING_LIST);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WAITING_LISTExists(wAITING_LIST.WaitingListID))
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
            return View(wAITING_LIST);
        }

        // GET: WAITING_LIST/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var wAITING_LIST = await _context.WAITING_LIST
                .FirstOrDefaultAsync(m => m.WaitingListID == id);
            if (wAITING_LIST == null)
            {
                return NotFound();
            }

            return View(wAITING_LIST);
        }

        // POST: WAITING_LIST/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var wAITING_LIST = await _context.WAITING_LIST.FindAsync(id);
            if (wAITING_LIST != null)
            {
                _context.WAITING_LIST.Remove(wAITING_LIST);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WAITING_LISTExists(int id)
        {
            return _context.WAITING_LIST.Any(e => e.WaitingListID == id);
        }
    }
}
