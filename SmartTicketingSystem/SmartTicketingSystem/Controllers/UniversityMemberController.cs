using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTicketingSystem.Data;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTicketingSystem.Controllers
{
    [Authorize(Policy = "UniversityMemberOnly")]
    public class UniversityMemberController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UniversityMemberController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            // 1) Find logged in app user
            var identityId = _userManager.GetUserId(User);
            var appUser = await _context.USER.FirstOrDefaultAsync(u => u.IdentityUserId == identityId);

            if (appUser == null)
            {
                ViewBag.MyBookings = 0;
                ViewBag.MyTickets = 0;
                ViewBag.MyPaymentsTotal = 0m;
                ViewBag.UpcomingEvents = 0;

                ViewData["BookingMonthLabels"] = new List<string>();
                ViewData["BookingMonthCounts"] = new List<int>();
                ViewData["MyRecentBookings"] = new List<object>();

                return View();
            }

            // 2) My bookings
            var myBookingsQuery = _context.BOOKING.Where(b => b.member_id == appUser.member_id);

            var myBookingIds = await myBookingsQuery.Select(b => b.BookingID).ToListAsync();

            ViewBag.MyBookings = myBookingIds.Count;

            // 3) My tickets (TICKET linked by BookingID)
            ViewBag.MyTickets = await _context.TICKET.CountAsync(t => myBookingIds.Contains(t.BookingID));

            // 4) My payments total (PAYMENT linked by BookingID)
            var myPaymentsTotal = await _context.PAYMENT
                .Where(p => myBookingIds.Contains(p.BookingID))
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;

            ViewBag.MyPaymentsTotal = myPaymentsTotal;

            // 5) Upcoming events count (future + Published)
            // if your EVENT.status values differ, adjust "Published"
            ViewBag.UpcomingEvents = await _context.EVENT.CountAsync(e =>
                e.StartDateTime >= DateTime.Now &&
                (e.status == "Published" || e.visibility == "Public"));

            // 6) Chart: My bookings per month (BookingDateTime)
            var bookingsPerMonth = await myBookingsQuery
                .GroupBy(b => b.BookingDateTime.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .OrderBy(x => x.Month)
                .ToListAsync();

            string[] monthNames = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

            ViewData["BookingMonthLabels"] = bookingsPerMonth
                .Select(x => monthNames[Math.Max(1, Math.Min(12, x.Month)) - 1])
                .ToList();

            ViewData["BookingMonthCounts"] = bookingsPerMonth.Select(x => x.Count).ToList();

            // 7) Recent bookings table
            var myRecentBookings = await myBookingsQuery
                .OrderByDescending(b => b.BookingDateTime)
                .Select(b => new
                {
                    b.BookingID,
                    b.BookingReference,
                    b.TotalAmount,
                    b.PaymentStatus,
                    b.BookingStatus,
                    b.BookingDateTime
                })
                .Take(6)
                .ToListAsync();

            ViewData["MyRecentBookings"] = myRecentBookings;

            return View();
        }
    }
}
