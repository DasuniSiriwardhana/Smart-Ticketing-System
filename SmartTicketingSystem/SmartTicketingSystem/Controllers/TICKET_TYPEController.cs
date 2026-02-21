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

        // ==========HELPER METHODS ====================
        private async Task<int?> GetCurrentMemberIdAsync()
        {
            var identityUserId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(identityUserId)) return null;

            return await _context.USER
                .Where(u => u.IdentityUserId == identityUserId)
                .Select(u => (int?)u.member_id)
                .FirstOrDefaultAsync();
        }

        private bool IsAdmin() => User.IsInRole("Admin");
        private bool IsOrganizer() => User.IsInRole("Organizer");

        private async Task<bool> CanManageEventTickets(int eventId)
        {
            if (IsAdmin()) return true;

            if (IsOrganizer())
            {
                var memberId = await GetCurrentMemberIdAsync();
                if (memberId == null) return false;

                var ev = await _context.EVENT.FindAsync(eventId);
                return ev != null && ev.createdByUserID == memberId.Value;
            }

            return false;
        }

        // ==========CREATE FOR EVENT (GET) ====================
        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> CreateForEvent(int eventId)
        {
            // Check if user can manage this event
            if (!await CanManageEventTickets(eventId))
            {
                return Forbid();
            }

            var event_details = await _context.EVENT.FindAsync(eventId);
            if (event_details == null) return NotFound();

            ViewBag.Event = event_details;

            // Default values - FIXED: Use endDateTime (lowercase) not EndDateTime
            var ticketType = new TICKET_TYPE
            {
                EventID = eventId,
                salesStartAt = DateTime.Now,
                // Fix: Check if endDateTime is default (not set) then use default + 1 month
                salesEndAt = event_details.endDateTime != default ? event_details.endDateTime : DateTime.Now.AddMonths(1),
                seatLimit = 100,
                Price = 0,
                isActive = 'Y'
            };

            return View(ticketType);
        }

        // ==========CREATE FOR EVENT (POST) ====================
        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateForEvent([Bind("EventID,TypeName,Price,seatLimit,salesStartAt,salesEndAt")] TICKET_TYPE ticketType)
        {
            // Check if user can manage this event
            if (!await CanManageEventTickets(ticketType.EventID))
            {
                return Forbid();
            }

            // Set system fields
            ticketType.createdAt = DateTime.Now;
            ticketType.isActive = 'Y';

            // Validate dates
            if (ticketType.salesEndAt <= ticketType.salesStartAt)
            {
                ModelState.AddModelError("salesEndAt", "End date must be after start date");
            }

            // Validate seat limit
            if (ticketType.seatLimit <= 0)
            {
                ModelState.AddModelError("seatLimit", "Seat limit must be greater than 0");
            }

            // Validate price
            if (ticketType.Price < 0)
            {
                ModelState.AddModelError("Price", "Price cannot be negative");
            }

            // Validate name
            if (string.IsNullOrWhiteSpace(ticketType.TypeName))
            {
                ModelState.AddModelError("TypeName", "Ticket type name is required");
            }

            if (ModelState.IsValid)
            {
                _context.Add(ticketType);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Ticket type '{ticketType.TypeName}' added successfully!";

                // Redirect back to the organizer tickets page
                return RedirectToAction("Tickets", "Organizer", new { eventId = ticketType.EventID });
            }

            // Reload event for view
            var event_details = await _context.EVENT.FindAsync(ticketType.EventID);
            ViewBag.Event = event_details;

            return View(ticketType);
        }

        // ==========TOGGLE STATUS ====================
        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var ticketType = await _context.TICKET_TYPE.FindAsync(id);
            if (ticketType == null) return NotFound();

            // Check if user can manage this event
            if (!await CanManageEventTickets(ticketType.EventID))
            {
                return Forbid();
            }

            ticketType.isActive = ticketType.isActive == 'Y' ? 'N' : 'Y';
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Ticket type {(ticketType.isActive == 'Y' ? "activated" : "deactivated")}";

            // Redirect back to the organizer tickets page
            return RedirectToAction("Tickets", "Organizer", new { eventId = ticketType.EventID });
        }

        // ==========SEARCH ====================
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
            var query = _context.TICKET_TYPE.AsQueryable();

            if (mode == "TicketID" && ticketId.HasValue)
                query = query.Where(t => t.TicketID == ticketId.Value);

            else if (mode == "EventID" && eventId.HasValue)
                query = query.Where(t => t.EventID == eventId.Value);

            else if (mode == "TypeName" && !string.IsNullOrWhiteSpace(typeName))
                query = query.Where(t => t.TypeName.Contains(typeName));

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
                if (!string.IsNullOrWhiteSpace(typeName)) query = query.Where(t => t.TypeName.Contains(typeName));

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

        // ==========INDEX ====================
        public async Task<IActionResult> Index()
        {
            return View(await _context.TICKET_TYPE.OrderByDescending(t => t.createdAt).ToListAsync());
        }

        // ==========DETAILS ====================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ticketType = await _context.TICKET_TYPE.FirstOrDefaultAsync(m => m.TicketID == id);
            if (ticketType == null) return NotFound();

            return View(ticketType);
        }

        // ==========CREATE (GET) ====================
        [Authorize(Policy = "AdminOrOrganizer")]
        public IActionResult Create() => View();

        // ==========CREATE (POST) ====================
        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventID,TypeName,Price,seatLimit,salesStartAt,salesEndAt")] TICKET_TYPE ticketType)
        {
            if (ticketType.createdAt == default)
                ticketType.createdAt = DateTime.Now;

            ticketType.isActive = 'Y';

            if (ModelState.IsValid)
            {
                _context.Add(ticketType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ticketType);
        }

        // ==========EDIT (GET) ====================
        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ticketType = await _context.TICKET_TYPE.FindAsync(id);
            if (ticketType == null) return NotFound();

            // Check if user can manage this event
            if (!await CanManageEventTickets(ticketType.EventID))
            {
                return Forbid();
            }

            var event_details = await _context.EVENT.FindAsync(ticketType.EventID);
            ViewBag.Event = event_details;

            return View(ticketType);
        }

        // ==========EDIT (POST) ====================
        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TicketID,EventID,TypeName,Price,seatLimit,salesStartAt,salesEndAt,isActive,createdAt")] TICKET_TYPE ticketType)
        {
            if (id != ticketType.TicketID) return NotFound();

            // Check if user can manage this event
            if (!await CanManageEventTickets(ticketType.EventID))
            {
                return Forbid();
            }

            // Check if any bookings exist for this ticket type
            var hasBookings = await _context.BOOKING_ITEM
                .AnyAsync(bi => bi.TicketTypeID == id);

            if (hasBookings)
            {
                var original = await _context.TICKET_TYPE.AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TicketID == id);

                if (original != null && original.seatLimit != ticketType.seatLimit)
                {
                    ModelState.AddModelError("seatLimit",
                        "Cannot change seat limit after tickets have been sold");
                }
            }

            // Validate dates
            if (ticketType.salesEndAt <= ticketType.salesStartAt)
            {
                ModelState.AddModelError("salesEndAt", "End date must be after start date");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticketType);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Ticket type updated successfully!";

                    // Redirect back to the organizer tickets page
                    return RedirectToAction("Tickets", "Organizer", new { eventId = ticketType.EventID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.TICKET_TYPE.Any(e => e.TicketID == id))
                        return NotFound();
                    throw;
                }
            }

            var event_details = await _context.EVENT.FindAsync(ticketType.EventID);
            ViewBag.Event = event_details;

            return View(ticketType);
        }

        // ==========DELETE (GET) ====================
        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var ticketType = await _context.TICKET_TYPE
                .FirstOrDefaultAsync(m => m.TicketID == id);

            if (ticketType == null) return NotFound();

            // Check if user can manage this event
            if (!await CanManageEventTickets(ticketType.EventID))
            {
                return Forbid();
            }

            // Check if any bookings exist
            var hasBookings = await _context.BOOKING_ITEM
                .AnyAsync(bi => bi.TicketTypeID == id);

            if (hasBookings)
            {
                TempData["Error"] = "Cannot delete ticket type with existing bookings. Deactivate it instead.";
                return RedirectToAction("Tickets", "Organizer", new { eventId = ticketType.EventID });
            }

            return View(ticketType);
        }

        // ==========DELETE (POST) ====================
        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticketType = await _context.TICKET_TYPE.FindAsync(id);
            if (ticketType == null) return NotFound();

            // Check if user can manage this event
            if (!await CanManageEventTickets(ticketType.EventID))
            {
                return Forbid();
            }

            // Double-check no bookings
            var hasBookings = await _context.BOOKING_ITEM
                .AnyAsync(bi => bi.TicketTypeID == id);

            if (hasBookings)
            {
                TempData["Error"] = "Cannot delete ticket type with existing bookings.";
                return RedirectToAction("Tickets", "Organizer", new { eventId = ticketType.EventID });
            }

            _context.TICKET_TYPE.Remove(ticketType);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Ticket type deleted successfully!";
            return RedirectToAction("Tickets", "Organizer", new { eventId = ticketType.EventID });
        }
    }
}