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
    [Authorize(Policy = "MemberOnly")]
    public class TICKET_TYPEController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TICKET_TYPEController(ApplicationDbContext context)
        {
            _context = context;
        }

        // SEARCH (members allowed)
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
                if (minPrice.HasValue) query = query.Where(t => t.Price >= minPrice.Value);
                if (maxPrice.HasValue) query = query.Where(t => t.Price <= maxPrice.Value);
            }

            else if (mode == "SeatRange")
            {
                if (minSeat.HasValue) query = query.Where(t => t.seatLimit >= minSeat.Value);
                if (maxSeat.HasValue) query = query.Where(t => t.seatLimit <= maxSeat.Value);
            }

            else if (mode == "SalesDateRange")
            {
                if (salesFrom.HasValue) query = query.Where(t => t.salesStartAt >= salesFrom.Value);
                if (salesTo.HasValue) query = query.Where(t => t.salesEndAt <= salesTo.Value);
            }

            else if (mode == "Status" && isActive.HasValue)
                query = query.Where(t => t.isActive == isActive.Value);

            else if (mode == "Advanced")
            {
                if (ticketId.HasValue) query = query.Where(t => t.TicketID == ticketId.Value);
                if (eventId.HasValue) query = query.Where(t => t.EventID == eventId.Value);
                if (!string.IsNullOrWhiteSpace(typeName)) query = query.Where(t => (t.TypeName ?? "").Contains(typeName));

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

            return View("Index", await query.OrderByDescending(t => t.createdAt).ToListAsync());
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.TICKET_TYPE.OrderByDescending(t => t.createdAt).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var tICKET_TYPE = await _context.TICKET_TYPE.FirstOrDefaultAsync(m => m.TicketID == id);
            if (tICKET_TYPE == null) return NotFound();

            return View(tICKET_TYPE);
        }

        // CREATE/EDIT/DELETE: AdminOrOrganizer only
        [Authorize(Policy = "AdminOrOrganizer")]
        public IActionResult Create() => View();

        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TicketID,EventID,TypeName,Price,seatLimit,salesStartAt,salesEndAt,isActive,createdAt")] TICKET_TYPE tICKET_TYPE)
        {
            if (tICKET_TYPE.createdAt == default) tICKET_TYPE.createdAt = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Add(tICKET_TYPE);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tICKET_TYPE);
        }

        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var tICKET_TYPE = await _context.TICKET_TYPE.FindAsync(id);
            if (tICKET_TYPE == null) return NotFound();

            return View(tICKET_TYPE);
        }

        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TicketID,EventID,TypeName,Price,seatLimit,salesStartAt,salesEndAt,isActive,createdAt")] TICKET_TYPE tICKET_TYPE)
        {
            if (id != tICKET_TYPE.TicketID) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(tICKET_TYPE);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(tICKET_TYPE);
        }

        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var tICKET_TYPE = await _context.TICKET_TYPE.FirstOrDefaultAsync(m => m.TicketID == id);
            if (tICKET_TYPE == null) return NotFound();

            return View(tICKET_TYPE);
        }

        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tICKET_TYPE = await _context.TICKET_TYPE.FindAsync(id);
            if (tICKET_TYPE != null)
                _context.TICKET_TYPE.Remove(tICKET_TYPE);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
