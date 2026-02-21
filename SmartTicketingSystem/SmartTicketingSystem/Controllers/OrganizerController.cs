using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTicketingSystem.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartTicketingSystem.Controllers
{
    // Anyone who has Organizer role can enter this controller,
    // but specific actions are restricted by composed policies.
    [Authorize(Policy = "OrganizerOnly")]
    public class OrganizerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public OrganizerController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Helper to get current member ID
        private async Task<int?> GetCurrentMemberIdAsync()
        {
            var identityId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(identityId)) return null;

            var appUser = await _context.USER.FirstOrDefaultAsync(u => u.IdentityUserId == identityId);
            return appUser?.member_id;
        }

        // University Organizer dashboard (Organizer + UniversityMember)
        [Authorize(Policy = "UniversityOrganizer")]
        public async Task<IActionResult> Dashboard()
        {
            // 1) Find current app user (custom USER table) using IdentityUserId
            var identityId = _userManager.GetUserId(User);
            var appUser = await _context.USER.FirstOrDefaultAsync(u => u.IdentityUserId == identityId);

            if (appUser == null)
            {
                // Safe empty dashboard (no crash)
                ViewBag.TotalMyEvents = 0;
                ViewBag.TotalBookingsForMyEvents = 0;
                ViewBag.TotalTicketsIssued = 0;
                ViewBag.UpcomingEvents = 0;

                ViewData["EventMonthLabels"] = new List<string>();
                ViewData["EventMonthCounts"] = new List<int>();

                ViewData["TopEventTitles"] = new List<string>();
                ViewData["TopEventBookingCounts"] = new List<int>();

                ViewData["MyRecentEvents"] = new List<object>();

                return View();
            }

            // 2) My events (Organizer created)
            var myEventsQuery = _context.EVENT.Where(e => e.createdByUserID == appUser.member_id);
            var myEventIds = await myEventsQuery.Select(e => e.eventID).ToListAsync();

            // Summary cards
            ViewBag.TotalMyEvents = myEventIds.Count;
            ViewBag.UpcomingEvents = await myEventsQuery.CountAsync(e => e.StartDateTime >= DateTime.Now);

            // 3) Bookings for MY events
            var bookingIdsForMyEvents = await _context.BOOKING_ITEM
                .Join(_context.TICKET_TYPE,
                    bi => bi.TicketTypeID,
                    tt => tt.TicketID,
                    (bi, tt) => new { bi, tt })
                .Where(x => myEventIds.Contains(x.tt.EventID))
                .Select(x => x.bi.BookingID)
                .Distinct()
                .ToListAsync();

            ViewBag.TotalBookingsForMyEvents = bookingIdsForMyEvents.Count;

            // 4) Tickets/Seats booked for MY events (sum quantities)
            var totalSeatsBooked = await _context.BOOKING_ITEM
                .Join(_context.TICKET_TYPE,
                    bi => bi.TicketTypeID,
                    tt => tt.TicketID,
                    (bi, tt) => new { bi, tt })
                .Where(x => myEventIds.Contains(x.tt.EventID))
                .SumAsync(x => (int?)x.bi.Quantity) ?? 0;

            ViewBag.TotalTicketsIssued = totalSeatsBooked;

            // 5) Chart: My events created per month
            var eventsPerMonth = await myEventsQuery
                .GroupBy(e => e.createdAt.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .OrderBy(x => x.Month)
                .ToListAsync();

            string[] monthNames = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

            ViewData["EventMonthLabels"] = eventsPerMonth
                .Select(x => monthNames[Math.Clamp(x.Month, 1, 12) - 1])
                .ToList();

            ViewData["EventMonthCounts"] = eventsPerMonth.Select(x => x.Count).ToList();

            // 6) Chart: Top events by bookings
            var topEvents = await _context.BOOKING_ITEM
                .Join(_context.TICKET_TYPE,
                    bi => bi.TicketTypeID,
                    tt => tt.TicketID,
                    (bi, tt) => new { bi, tt })
                .Where(x => myEventIds.Contains(x.tt.EventID))
                .GroupBy(x => x.tt.EventID)
                .Select(g => new
                {
                    EventID = g.Key,
                    BookingCount = g.Select(z => z.bi.BookingID).Distinct().Count()
                })
                .OrderByDescending(x => x.BookingCount)
                .Take(5)
                .ToListAsync();

            var topEventIds = topEvents.Select(x => x.EventID).ToList();
            var topTitlesMap = await _context.EVENT
                .Where(e => topEventIds.Contains(e.eventID))
                .Select(e => new { e.eventID, e.title })
                .ToListAsync();

            ViewData["TopEventTitles"] = topEvents
                .Select(x => topTitlesMap.FirstOrDefault(t => t.eventID == x.EventID)?.title ?? $"Event {x.EventID}")
                .ToList();

            ViewData["TopEventBookingCounts"] = topEvents.Select(x => x.BookingCount).ToList();

            // 7) Recent events table
            var myRecentEvents = await myEventsQuery
                .OrderByDescending(e => e.createdAt)
                .Select(e => new
                {
                    e.eventID,
                    e.title,
                    e.StartDateTime,
                    e.status,
                    e.capacity
                })
                .Take(6)
                .ToListAsync();

            ViewData["MyRecentEvents"] = myRecentEvents;

            return View();
        }

        // =======TICKET MANAGEMENT =================
        [Authorize(Policy = "UniversityOrganizer")]
        public async Task<IActionResult> Tickets(int eventId)
        {
            var memberId = await GetCurrentMemberIdAsync();
            if (memberId == null) return Forbid();

            var ev = await _context.EVENT
                .FirstOrDefaultAsync(e => e.eventID == eventId && e.createdByUserID == memberId.Value);

            if (ev == null) return NotFound();

            var ticketTypes = await _context.TICKET_TYPE
                .Where(t => t.EventID == eventId)
                .OrderBy(t => t.Price)
                .ToListAsync();

            // Get sold counts with null handling
            var soldCounts = new Dictionary<int, int>();
            var soldData = await _context.BOOKING_ITEM
                .Include(bi => bi.Booking)
                .Where(bi => bi.Booking != null && bi.Booking.EventID == eventId)
                .GroupBy(bi => bi.TicketTypeID)
                .Select(g => new { TicketTypeID = g.Key, Sold = g.Sum(bi => bi.Quantity) })
                .ToListAsync();

            foreach (var item in soldData)
            {
                soldCounts[item.TicketTypeID] = item.Sold;
            }

            ViewBag.Event = ev;
            ViewBag.SoldCounts = soldCounts;

            return View(ticketTypes);
        }

        // =======PROMOTION MANAGEMENT =================
        [Authorize(Policy = "UniversityOrganizer")]
        public async Task<IActionResult> Promotions(int eventId)
        {
            var memberId = await GetCurrentMemberIdAsync();
            if (memberId == null) return Forbid();

            var ev = await _context.EVENT
                .FirstOrDefaultAsync(e => e.eventID == eventId && e.createdByUserID == memberId.Value);

            if (ev == null) return NotFound();

            // Get promos used for this event
            var usedPromos = await _context.BOOKING_PROMO
                .Include(bp => bp.PromoCode)
                .Where(bp => bp.Booking != null && bp.Booking.EventID == eventId && bp.PromoCode != null)
                .Select(bp => bp.PromoCode)
                .Distinct()
                .ToListAsync();

            // Get used promo IDs
            var usedPromoIds = usedPromos
                .Where(p => p != null)
                .Select(p => p.PromoCodeID)
                .ToHashSet();

            // Get available promos
            var availablePromos = await _context.PROMO_CODE
                .Where(p => p.isActive == 'Y')
                .ToListAsync();

            // Filter out used ones
            availablePromos = availablePromos
                .Where(p => !usedPromoIds.Contains(p.PromoCodeID))
                .ToList();

            ViewBag.UsedPromos = usedPromos;
            ViewBag.AvailablePromos = availablePromos;

            // IMPORTANT: Pass the event as the model
            return View(ev);
        }

        // =======BOOKINGS FOR EVENT =================
        [Authorize(Policy = "UniversityOrganizer")]
        public async Task<IActionResult> Bookings(int eventId)
        {
            var memberId = await GetCurrentMemberIdAsync();
            if (memberId == null) return Forbid();

            var ev = await _context.EVENT
                .FirstOrDefaultAsync(e => e.eventID == eventId && e.createdByUserID == memberId.Value);

            if (ev == null) return NotFound();

            var bookings = await _context.BOOKING
                .Include(b => b.User)
                .Include(b => b.BookingItems)
                    .ThenInclude(bi => bi.TicketType)
                .Where(b => b.EventID == eventId)
                .OrderByDescending(b => b.BookingDateTime)
                .ToListAsync();

            ViewBag.Event = ev;

            return View(bookings);
        }

        // =======WAITING LIST =================
        [Authorize(Policy = "UniversityOrganizer")]
        public async Task<IActionResult> WaitingList(int eventId)
        {
            var memberId = await GetCurrentMemberIdAsync();
            if (memberId == null) return Forbid();

            var ev = await _context.EVENT
                .FirstOrDefaultAsync(e => e.eventID == eventId && e.createdByUserID == memberId.Value);

            if (ev == null) return NotFound();

            var waitingList = await _context.WAITING_LIST
                .Include(w => w.User)
                .Where(w => w.EventID == eventId)
                .OrderBy(w => w.AddedAt)
                .ToListAsync();

            ViewBag.Event = ev;

            return View(waitingList);
        }

        // =======EVENT REPORTS (UPDATED WITH NULL SAFETY) =================
        [Authorize(Policy = "UniversityOrganizer")]
        public async Task<IActionResult> Reports(int eventId)
        {
            var memberId = await GetCurrentMemberIdAsync();
            if (memberId == null) return Forbid();

            var ev = await _context.EVENT
                .FirstOrDefaultAsync(e => e.eventID == eventId && e.createdByUserID == memberId.Value);

            if (ev == null) return NotFound();

            // Daily sales for last 30 days
            var startDate = DateTime.Now.AddDays(-30);
            var dailySales = await _context.BOOKING
                .Where(b => b.EventID == eventId && b.BookingDateTime >= startDate)
                .GroupBy(b => b.BookingDateTime.Date)
                .Select(g => new {
                    Date = g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(b => b.TotalAmount)
                })
                .OrderBy(g => g.Date)
                .ToListAsync();

            // Ticket breakdown with null handling
            var ticketBreakdown = await _context.BOOKING_ITEM
                .Include(bi => bi.TicketType)
                .Where(bi => bi.Booking != null && bi.Booking.EventID == eventId)
                .GroupBy(bi => new {
                    TicketTypeID = bi.TicketTypeID,
                    TypeName = bi.TicketType != null ? bi.TicketType.TypeName : "Unknown"
                })
                .Select(g => new
                {
                    TicketType = g.Key.TypeName,
                    Sold = g.Sum(bi => bi.Quantity),
                    Revenue = g.Sum(bi => bi.LineTotal)
                })
                .ToListAsync();

            // Promo effectiveness with null handling
            var promoEffectiveness = await _context.BOOKING_PROMO
                .Include(bp => bp.PromoCode)
                .Where(bp => bp.Booking != null && bp.Booking.EventID == eventId && bp.PromoCode != null)
                .GroupBy(bp => new {
                    BookingCodeID = bp.BookingCodeID,
                    Code = bp.PromoCode != null ? bp.PromoCode.code : "Unknown"
                })
                .Select(g => new
                {
                    PromoCode = g.Key.Code,
                    TimesUsed = g.Count(),
                    TotalDiscount = g.Sum(bp => bp.DiscountedAmount)
                })
                .ToListAsync();

            ViewBag.Event = ev;
            ViewBag.DailySales = dailySales;
            ViewBag.TicketBreakdown = ticketBreakdown;
            ViewBag.PromoEffectiveness = promoEffectiveness;

            return View();
        }

        // External Organizer dashboard (Organizer + ExternalMember)
        [Authorize(Policy = "ExternalOrganizer")]
        public IActionResult ExternalDashboard()
        {
            return View();
        }

        // Optional helper route
        [HttpGet("/Organizer")]
        public IActionResult Index()
        {
            if (User.IsInRole("Organizer") && User.IsInRole("UniversityMember"))
                return RedirectToAction(nameof(Dashboard));

            if (User.IsInRole("Organizer") && User.IsInRole("ExternalMember"))
                return RedirectToAction(nameof(ExternalDashboard));

            return RedirectToAction("Index", "Home");
        }
    }
}