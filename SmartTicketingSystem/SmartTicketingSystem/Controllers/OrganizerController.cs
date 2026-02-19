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
            // EVENT.createdByUserID stores USER.member_id of organizer
            var myEventsQuery = _context.EVENT.Where(e => e.createdByUserID == appUser.member_id);

            var myEventIds = await myEventsQuery.Select(e => e.eventID).ToListAsync();

            // Summary cards
            ViewBag.TotalMyEvents = myEventIds.Count;
            ViewBag.UpcomingEvents = await myEventsQuery.CountAsync(e => e.StartDateTime >= DateTime.Now);

            // 3) Bookings for MY events
            // BOOKING_ITEM.TicketTypeID -> TICKET_TYPE.TicketID
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

            // 5) Chart: My events created per month (EVENT.createdAt)
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

            // 6) Chart: Top events by bookings (distinct bookings per event)
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

        // External Organizer dashboard (Organizer + ExternalMember)
        [Authorize(Policy = "ExternalOrganizer")]
        public IActionResult ExternalDashboard()
        {
            // This will load: Views/Organizer/ExternalDashboard.cshtml
            return View();
        }

        // Optional helper route: /Organizer -> sends them to correct dashboard
        // (Only if you want. If you don’t need it, you can delete this method.)
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
