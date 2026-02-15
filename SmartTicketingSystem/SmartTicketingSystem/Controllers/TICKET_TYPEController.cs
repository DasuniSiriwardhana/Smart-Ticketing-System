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
    public class TICKET_TYPEController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TICKET_TYPEController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Search Bar Option

public async Task<IActionResult> Search(
    string mode,
    int? ticketId,
    int? eventId,
    string typeName,
    decimal? minPrice,
    decimal? maxPrice,
    int? minSeat,
    int? maxSeat,
    char? isActive,
    DateTime? salesFrom,
    DateTime? salesTo,
    DateTime? createdFrom,
    DateTime? createdTo)
    {
        var query = _context.Set<TICKET_TYPE>().AsQueryable();

        if (mode == "TicketID" && ticketId.HasValue)
            query = query.Where(t => t.TicketID == ticketId.Value);

        else if (mode == "EventID" && eventId.HasValue)
            query = query.Where(t => t.EventID == eventId.Value);

        else if (mode == "TypeName" && !string.IsNullOrWhiteSpace(typeName))
            query = query.Where(t => (t.TypeName ?? "").Contains(typeName));

        else if (mode == "PriceRange")
        {
            if (minPrice.HasValue)
                query = query.Where(t => t.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(t => t.Price <= maxPrice.Value);
        }

        else if (mode == "SeatRange")
        {
            if (minSeat.HasValue)
                query = query.Where(t => t.seatLimit >= minSeat.Value);

            if (maxSeat.HasValue)
                query = query.Where(t => t.seatLimit <= maxSeat.Value);
        }

        else if (mode == "SalesDateRange")
        {
            if (salesFrom.HasValue)
                query = query.Where(t => t.salesStartAt >= salesFrom.Value);

            if (salesTo.HasValue)
                query = query.Where(t => t.salesEndAt <= salesTo.Value);
        }

        else if (mode == "Status" && isActive.HasValue)
            query = query.Where(t => t.isActive == isActive.Value);

        else if (mode == "Advanced")
        {
            if (ticketId.HasValue) query = query.Where(t => t.TicketID == ticketId.Value);
            if (eventId.HasValue) query = query.Where(t => t.EventID == eventId.Value);

            if (!string.IsNullOrWhiteSpace(typeName))
                query = query.Where(t => (t.TypeName ?? "").Contains(typeName));

            if (minPrice.HasValue) query = query.Where(t => t.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(t => t.Price <= maxPrice.Value);

            if (minSeat.HasValue) query = query.Where(t => t.seatLimit >= minSeat.Value);
            if (maxSeat.HasValue) query = query.Where(t => t.seatLimit <= maxSeat.Value);

            if (isActive.HasValue) query = query.Where(t => t.isActive == isActive.Value);

            if (salesFrom.HasValue) query = query.Where(t => t.salesStartAt >= salesFrom.Value);
            if (salesTo.HasValue) query = query.Where(t => t.salesEndAt <= salesTo.Value);

            if (createdFrom.HasValue) query = query.Where(t => t.createdAt >= createdFrom.Value);
            if (createdTo.HasValue) query = query.Where(t => t.createdAt < createdTo.Value.AddDays(1));
        }

        return View("Index", await query
            .OrderByDescending(t => t.createdAt)
            .ToListAsync());
    }


    // GET: TICKET_TYPE
    public async Task<IActionResult> Index()
        {
            return View(await _context.TICKET_TYPE.ToListAsync());
        }

        // GET: TICKET_TYPE/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tICKET_TYPE = await _context.TICKET_TYPE
                .FirstOrDefaultAsync(m => m.TicketID == id);
            if (tICKET_TYPE == null)
            {
                return NotFound();
            }

            return View(tICKET_TYPE);
        }

        // GET: TICKET_TYPE/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TICKET_TYPE/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TicketID,EventID,TypeName,Price,seatLimit,salesStartAt,salesEndAt,isActive,createdAt")] TICKET_TYPE tICKET_TYPE)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tICKET_TYPE);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tICKET_TYPE);
        }

        // GET: TICKET_TYPE/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tICKET_TYPE = await _context.TICKET_TYPE.FindAsync(id);
            if (tICKET_TYPE == null)
            {
                return NotFound();
            }
            return View(tICKET_TYPE);
        }

        // POST: TICKET_TYPE/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TicketID,EventID,TypeName,Price,seatLimit,salesStartAt,salesEndAt,isActive,createdAt")] TICKET_TYPE tICKET_TYPE)
        {
            if (id != tICKET_TYPE.TicketID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tICKET_TYPE);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TICKET_TYPEExists(tICKET_TYPE.TicketID))
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
            return View(tICKET_TYPE);
        }

        // GET: TICKET_TYPE/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tICKET_TYPE = await _context.TICKET_TYPE
                .FirstOrDefaultAsync(m => m.TicketID == id);
            if (tICKET_TYPE == null)
            {
                return NotFound();
            }

            return View(tICKET_TYPE);
        }

        // POST: TICKET_TYPE/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tICKET_TYPE = await _context.TICKET_TYPE.FindAsync(id);
            if (tICKET_TYPE != null)
            {
                _context.TICKET_TYPE.Remove(tICKET_TYPE);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TICKET_TYPEExists(int id)
        {
            return _context.TICKET_TYPE.Any(e => e.TicketID == id);
        }
    }
}
