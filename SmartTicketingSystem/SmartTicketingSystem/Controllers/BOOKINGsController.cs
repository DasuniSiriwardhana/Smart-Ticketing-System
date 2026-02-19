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
    public class BOOKINGsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BOOKINGsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helpers
        private async Task<int?> GetCurrentMemberIdAsync()
        {
            var identityUserId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(identityUserId)) return null;

            return await _context.USER
                .Where(u => u.IdentityUserId == identityUserId)
                .Select(u => (int?)u.member_id)
                .FirstOrDefaultAsync();
        }

        private async Task<bool> CurrentUserHasAnyRoleAsync(params string[] roleNames)
        {
            var memberId = await GetCurrentMemberIdAsync();
            if (memberId == null) return false;

            return await (from ur in _context.USER_ROLE
                          join r in _context.Role on ur.roleID equals r.RoleId
                          where ur.member_id == memberId.Value && roleNames.Contains(r.rolename)
                          select ur.UserRoleID).AnyAsync();
        }

        // SEARCH
        // Admin can search all
        // Members can only search their own bookings
        public async Task<IActionResult> Search(
            string mode,
            int? bookingId,
            int? member_id,
            int? eventId,
            string bookingStatus,
            string paymentStatus,
            string bookingReference)
        {
            var query = _context.Set<BOOKING>().AsQueryable();

            var isAdmin = await CurrentUserHasAnyRoleAsync("Admin");
            var myMemberId = await GetCurrentMemberIdAsync();

            if (!isAdmin)
            {
                if (myMemberId == null) return Forbid();
                query = query.Where(b => b.member_id == myMemberId.Value);
            }

            if (mode == "BookingID" && bookingId.HasValue)
                query = query.Where(b => b.BookingID == bookingId.Value);

            else if (mode == "MemberID" && member_id.HasValue)
                query = query.Where(b => b.member_id == member_id.Value);

            else if (mode == "EventID" && eventId.HasValue)
                query = query.Where(b => b.EventID == eventId.Value);

            else if (mode == "BookingStatus" && !string.IsNullOrWhiteSpace(bookingStatus))
                query = query.Where(b => (b.BookingStatus ?? "").Contains(bookingStatus));

            else if (mode == "PaymentStatus" && !string.IsNullOrWhiteSpace(paymentStatus))
                query = query.Where(b => (b.PaymentStatus ?? "").Contains(paymentStatus));

            else if (mode == "Reference" && !string.IsNullOrWhiteSpace(bookingReference))
                query = query.Where(b => (b.BookingReference ?? "").Contains(bookingReference));

            else if (mode == "Advanced")
            {
                if (bookingId.HasValue) query = query.Where(b => b.BookingID == bookingId.Value);
                if (member_id.HasValue) query = query.Where(b => b.member_id == member_id.Value);
                if (eventId.HasValue) query = query.Where(b => b.EventID == eventId.Value);

                if (!string.IsNullOrWhiteSpace(bookingStatus))
                    query = query.Where(b => (b.BookingStatus ?? "").Contains(bookingStatus));

                if (!string.IsNullOrWhiteSpace(paymentStatus))
                    query = query.Where(b => (b.PaymentStatus ?? "").Contains(paymentStatus));

                if (!string.IsNullOrWhiteSpace(bookingReference))
                    query = query.Where(b => (b.BookingReference ?? "").Contains(bookingReference));
            }

            return View("Index", await query.ToListAsync());
        }

        // INDEX
        public async Task<IActionResult> Index()
        {
            var isAdmin = await CurrentUserHasAnyRoleAsync("Admin");
            if (isAdmin)
                return View(await _context.BOOKING.ToListAsync());

            var myMemberId = await GetCurrentMemberIdAsync();
            if (myMemberId == null) return Forbid();

            return View(await _context.BOOKING
                .Where(b => b.member_id == myMemberId.Value)
                .ToListAsync());
        }
        // =========================
        // CONFIRMATION PAGE
        // =========================
        public async Task<IActionResult> Confirmation(int id)
        {
            var booking = await _context.BOOKING.FirstOrDefaultAsync(b => b.BookingID == id);
            if (booking == null) return NotFound();

            // Admin check (your custom role system)
            var isAdmin = await CurrentUserHasAnyRoleAsync("Admin");
            if (!isAdmin)
            {
                var myMemberId = await GetCurrentMemberIdAsync();
                if (myMemberId == null || booking.member_id != myMemberId.Value) return Forbid();
            }

            var items = await _context.BOOKING_ITEM
                .Where(bi => bi.BookingID == booking.BookingID)
                .Join(_context.TICKET_TYPE,
                    bi => bi.TicketTypeID,
                    tt => tt.TicketID,
                    (bi, tt) => new
                    {
                        tt.TypeName,
                        bi.Quantity,
                        bi.UnitPrice,
                        bi.LineTotal
                    })
                .ToListAsync();

            ViewData["Items"] = items;

            return View(booking);
        }


        // DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.BOOKING.FirstOrDefaultAsync(m => m.BookingID == id);
            if (booking == null) return NotFound();

            var isAdmin = await CurrentUserHasAnyRoleAsync("Admin");
            if (!isAdmin)
            {
                var myMemberId = await GetCurrentMemberIdAsync();
                if (myMemberId == null || booking.member_id != myMemberId.Value) return Forbid();
            }

            return View(booking);
        }

        // CREATE (Members can create their own booking)
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BOOKING booking)
        {
            var myMemberId = await GetCurrentMemberIdAsync();
            if (myMemberId == null) return Forbid();

            // 🔒 FORCE ownership + safe defaults
            booking.member_id = myMemberId.Value;
            booking.createdAt = DateTime.Now;

            // If not provided, set safe defaults
            if (booking.BookingDateTime == default) booking.BookingDateTime = DateTime.Now;
            booking.BookingStatus ??= "Pending";
            booking.PaymentStatus ??= "Unpaid";

            // Optional safe default for CancelledAt (avoid min date issues)
            if (booking.CancelledAt == default) booking.CancelledAt = DateTime.MinValue;

            if (!ModelState.IsValid)
                return View(booking);

            _context.Add(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // EDIT (Admin OR owner can access GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.BOOKING.FindAsync(id);
            if (booking == null) return NotFound();

            var isAdmin = await CurrentUserHasAnyRoleAsync("Admin");
            if (!isAdmin)
            {
                var myMemberId = await GetCurrentMemberIdAsync();
                if (myMemberId == null || booking.member_id != myMemberId.Value) return Forbid();
            }

            return View(booking);
        }

        // EDIT (POST) — ✅ upgraded to prevent members changing sensitive fields
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var dbBooking = await _context.BOOKING.FirstOrDefaultAsync(b => b.BookingID == id);
            if (dbBooking == null) return NotFound();

            var isAdmin = await CurrentUserHasAnyRoleAsync("Admin");

            if (!isAdmin)
            {
                var myMemberId = await GetCurrentMemberIdAsync();
                if (myMemberId == null || dbBooking.member_id != myMemberId.Value) return Forbid();

                // ✅ Members are only allowed to edit SAFE fields
                // (This blocks changing member_id, TotalAmount, PaymentStatus, etc.)
                var ok = await TryUpdateModelAsync(dbBooking, "",
                    b => b.BookingReference,
                    b => b.BookingStatus,
                    b => b.CancellationReason,
                    b => b.CancelledAt);

                if (!ok) return View(dbBooking);

                // 🔒 force safe values
                dbBooking.member_id = myMemberId.Value;
                dbBooking.createdAt = dbBooking.createdAt == default ? DateTime.Now : dbBooking.createdAt;
            }
            else
            {
                // ✅ Admin can update more fields
                var ok = await TryUpdateModelAsync(dbBooking, "",
                    b => b.BookingReference,
                    b => b.member_id,
                    b => b.EventID,
                    b => b.BookingDateTime,
                    b => b.BookingStatus,
                    b => b.TotalAmount,
                    b => b.PaymentStatus,
                    b => b.CancellationReason,
                    b => b.CancelledAt,
                    b => b.createdAt);

                if (!ok) return View(dbBooking);
            }

            if (!ModelState.IsValid)
                return View(dbBooking);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // DELETE (Admin only)
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.BOOKING.FirstOrDefaultAsync(m => m.BookingID == id);
            if (booking == null) return NotFound();

            return View(booking);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.BOOKING.FindAsync(id);
            if (booking != null) _context.BOOKING.Remove(booking);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
