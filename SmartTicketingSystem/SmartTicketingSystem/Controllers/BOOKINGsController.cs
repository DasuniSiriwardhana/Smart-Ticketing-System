using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTicketingSystem.Data;
using SmartTicketingSystem.Models;
using SmartTicketingSystem.Models.ViewModels;

namespace SmartTicketingSystem.Controllers
{
    [Authorize] // Just requires login, no specific role
    public class BOOKINGsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BOOKINGsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper to get current user's member ID
        private async Task<int?> GetCurrentMemberIdAsync()
        {
            var identityUserId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(identityUserId)) return null;

            return await _context.USER
                .Where(u => u.IdentityUserId == identityUserId)
                .Select(u => (int?)u.member_id)
                .FirstOrDefaultAsync();
        }

        // =========================
        // MY BOOKINGS - Show user's own bookings
        // =========================
        public async Task<IActionResult> MyBookings()
        {
            try
            {
                var memberId = await GetCurrentMemberIdAsync();
                if (memberId == null)
                {
                    TempData["Error"] = "Please login to view your bookings.";
                    return RedirectToAction("Login", "Account");
                }

                // Remove the .Include(b => b.Tickets) that's causing the error
                var bookings = await _context.BOOKING
                    .Include(b => b.Event)
                    .Include(b => b.BookingItems)
                        .ThenInclude(bi => bi.TicketType)
                    .Include(b => b.Payments)
                    .Where(b => b.member_id == memberId.Value)
                    .OrderByDescending(b => b.BookingDateTime)
                    .ToListAsync();

                return View(bookings);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading bookings: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // =========================
        // DETAILS - Show booking details
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var memberId = await GetCurrentMemberIdAsync();
            if (memberId == null) return Forbid();

            var booking = await _context.BOOKING
                .Include(b => b.Event)
                .Include(b => b.BookingItems)
                    .ThenInclude(bi => bi.TicketType)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.BookingID == id && b.member_id == memberId.Value);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // =========================
        // CANCEL - Only if not paid
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string cancellationReason)
        {
            var memberId = await GetCurrentMemberIdAsync();
            if (memberId == null) return Forbid();

            var booking = await _context.BOOKING
                .FirstOrDefaultAsync(b => b.BookingID == id && b.member_id == memberId.Value);

            if (booking == null) return NotFound();

            // CANCEL ONLY IF NOT PAID
            if (booking.PaymentStatus == "Paid")
            {
                TempData["Error"] = "Cannot cancel a paid booking. Please contact support.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Only allow cancellation if booking is in cancellable state
            if (booking.BookingStatus == "PendingPayment" || booking.BookingStatus == "Confirmed")
            {
                booking.BookingStatus = "Cancelled";
                booking.CancellationReason = cancellationReason;
                booking.CancelledAt = DateTime.Now;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Booking cancelled successfully.";
            }
            else
            {
                TempData["Error"] = "This booking cannot be cancelled.";
            }

            return RedirectToAction(nameof(MyBookings));
        }

        // =========================
        // CONFIRMATION - After booking
        // =========================
        public async Task<IActionResult> Confirmation(int id)
        {
            var memberId = await GetCurrentMemberIdAsync();
            if (memberId == null) return Forbid();

            var booking = await _context.BOOKING
                .Include(b => b.Event)
                .Include(b => b.BookingItems)
                    .ThenInclude(bi => bi.TicketType)
                .FirstOrDefaultAsync(b => b.BookingID == id && b.member_id == memberId.Value);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // =========================
        // TICKETS - View tickets for a booking
        // =========================
        public async Task<IActionResult> Tickets(int id)
        {
            var memberId = await GetCurrentMemberIdAsync();
            if (memberId == null) return Forbid();

            var booking = await _context.BOOKING
                .Include(b => b.Event)
                .FirstOrDefaultAsync(b => b.BookingID == id && b.member_id == memberId.Value);

            if (booking == null) return NotFound();

            var tickets = await _context.TICKET
                .Where(t => t.BookingID == id)
                .ToListAsync();

            ViewBag.Booking = booking;
            return View(tickets);
        }
    }
}