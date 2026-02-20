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
    public class BOOKING_PROMOController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BOOKING_PROMOController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // Helpers
        // =========================
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

        // =========================
        // INDEX - List all promo usages (Admin/Organizer only)
        // =========================
        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Index(int? eventId = null, int? promoId = null)
        {
            IQueryable<BOOKING_PROMO> query = _context.BOOKING_PROMO
                .Include(bp => bp.PromoCode)
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.Event)
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.User)
                .OrderByDescending(bp => bp.AppliedAt);

            // Filter by event
            if (eventId.HasValue)
            {
                query = query.Where(bp => bp.Booking.EventID == eventId.Value);
                var ev = await _context.EVENT.FindAsync(eventId.Value);
                ViewBag.Event = ev;
            }

            // Filter by promo
            if (promoId.HasValue)
            {
                query = query.Where(bp => bp.BookingCodeID == promoId.Value);
                var promo = await _context.PROMO_CODE.FindAsync(promoId.Value);
                ViewBag.Promo = promo;
            }

            // If organizer (not admin), only show their events
            if (!IsAdmin() && IsOrganizer())
            {
                var memberId = await GetCurrentMemberIdAsync();
                if (memberId == null) return Forbid();

                var organizerEvents = await _context.EVENT
                    .Where(e => e.createdByUserID == memberId.Value)
                    .Select(e => e.eventID)
                    .ToListAsync();

                query = query.Where(bp => organizerEvents.Contains(bp.Booking.EventID));
            }

            var bookingPromos = await query.ToListAsync();

            // Calculate summary stats
            ViewBag.TotalUsage = bookingPromos.Count;
            ViewBag.TotalDiscount = bookingPromos.Sum(bp => bp.DiscountedAmount);
            ViewBag.AverageDiscount = bookingPromos.Any() ? bookingPromos.Average(bp => bp.DiscountedAmount) : 0;

            return View(bookingPromos);
        }

        // =========================
        // DETAILS
        // =========================
        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var bookingPromo = await _context.BOOKING_PROMO
                .Include(bp => bp.PromoCode)
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.Event)
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.User)
                .FirstOrDefaultAsync(m => m.BookingPromoID == id);

            if (bookingPromo == null) return NotFound();

            // Check permission
            if (!IsAdmin() && IsOrganizer())
            {
                var memberId = await GetCurrentMemberIdAsync();
                if (memberId == null) return Forbid();

                var ev = await _context.EVENT.FindAsync(bookingPromo.Booking.EventID);
                if (ev == null || ev.createdByUserID != memberId.Value)
                    return Forbid();
            }

            return View(bookingPromo);
        }

        // =========================
        // CREATE (GET)
        // =========================
        [Authorize(Policy = "AdminOrOrganizer")]
        public IActionResult Create(int? bookingId = null)
        {
            var bookingPromo = new BOOKING_PROMO
            {
                BookingID = bookingId ?? 0,
                AppliedAt = DateTime.Now
            };

            if (bookingId.HasValue)
            {
                var booking = _context.BOOKING
                    .Include(b => b.Event)
                    .FirstOrDefault(b => b.BookingID == bookingId.Value);
                ViewBag.Booking = booking;
            }

            ViewBag.PromoCodes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _context.PROMO_CODE.Where(p => p.isActive == 'Y'),
                "PromoCodeID", "code");

            return View(bookingPromo);
        }

        // =========================
        // CREATE (POST)
        // =========================
        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BOOKING_PROMO bookingPromo)
        {
            // Check if promo code exists and is active
            var promo = await _context.PROMO_CODE
                .FirstOrDefaultAsync(p => p.PromoCodeID == bookingPromo.BookingCodeID
                    && p.isActive == 'Y');

            if (promo == null)
            {
                ModelState.AddModelError("BookingCodeID", "Invalid or inactive promo code");
            }

            // Check if booking exists
            var booking = await _context.BOOKING
                .Include(b => b.Event)
                .FirstOrDefaultAsync(b => b.BookingID == bookingPromo.BookingID);

            if (booking == null)
            {
                ModelState.AddModelError("BookingID", "Booking not found");
            }

            // Check if promo already applied to this booking
            var exists = await _context.BOOKING_PROMO
                .AnyAsync(bp => bp.BookingID == bookingPromo.BookingID);

            if (exists)
            {
                ModelState.AddModelError("", "A promo code has already been applied to this booking");
            }

            // Check if booking is eligible for promo (not paid, not cancelled)
            if (booking != null && (booking.PaymentStatus == "Paid" || booking.BookingStatus == "Cancelled"))
            {
                ModelState.AddModelError("", "Cannot apply promo to paid or cancelled bookings");
            }

            // Validate dates
            if (promo != null)
            {
                var now = DateTime.Now;
                if (now < promo.startDate || now > promo.endDate)
                {
                    ModelState.AddModelError("", "This promo code is not valid at this time");
                }
            }

            if (ModelState.IsValid)
            {
                // Calculate discount
                decimal discountAmount = 0;
                if (promo.DiscountType == "Percentage")
                {
                    discountAmount = booking.TotalAmount * (promo.DiscountValue / 100);
                }
                else
                {
                    discountAmount = promo.DiscountValue;
                }

                // Ensure discount doesn't exceed total
                discountAmount = Math.Min(discountAmount, booking.TotalAmount);

                bookingPromo.DiscountedAmount = discountAmount;
                bookingPromo.AppliedAt = DateTime.Now;

                _context.Add(bookingPromo);

                // Update booking total
                booking.TotalAmount -= discountAmount;
                _context.Update(booking);

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Promo code '{promo.code}' applied successfully! Discount: {discountAmount:C}";
                return RedirectToAction("Details", "BOOKINGs", new { id = booking.BookingID });
            }

            ViewBag.PromoCodes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                _context.PROMO_CODE.Where(p => p.isActive == 'Y'),
                "PromoCodeID", "code", bookingPromo.BookingCodeID);

            if (booking != null)
                ViewBag.Booking = booking;

            return View(bookingPromo);
        }

        // =========================
        // EDIT (GET) - Admin only
        // =========================
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var bookingPromo = await _context.BOOKING_PROMO
                .Include(bp => bp.Booking)
                .FirstOrDefaultAsync(bp => bp.BookingPromoID == id);

            if (bookingPromo == null) return NotFound();

            return View(bookingPromo);
        }

        // =========================
        // EDIT (POST) - Admin only
        // =========================
        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BOOKING_PROMO bookingPromo)
        {
            if (id != bookingPromo.BookingPromoID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bookingPromo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.BOOKING_PROMO.Any(e => e.BookingPromoID == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(bookingPromo);
        }

        // =========================
        // DELETE (GET) - Admin only
        // =========================
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var bookingPromo = await _context.BOOKING_PROMO
                .Include(bp => bp.PromoCode)
                .Include(bp => bp.Booking)
                .FirstOrDefaultAsync(m => m.BookingPromoID == id);

            if (bookingPromo == null) return NotFound();

            return View(bookingPromo);
        }

        // =========================
        // DELETE (POST) - Admin only
        // =========================
        [Authorize(Policy = "AdminOnly")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bookingPromo = await _context.BOOKING_PROMO
                .Include(bp => bp.Booking)
                .FirstOrDefaultAsync(bp => bp.BookingPromoID == id);

            if (bookingPromo != null)
            {
                // Restore original booking total
                var booking = bookingPromo.Booking;
                booking.TotalAmount += bookingPromo.DiscountedAmount;
                _context.Update(booking);

                _context.BOOKING_PROMO.Remove(bookingPromo);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Promo code removed from booking";
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // REMOVE FROM BOOKING - For users to remove promo
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromBooking(int bookingId)
        {
            var memberId = await GetCurrentMemberIdAsync();
            if (memberId == null) return Forbid();

            var bookingPromo = await _context.BOOKING_PROMO
                .Include(bp => bp.Booking)
                .FirstOrDefaultAsync(bp => bp.BookingID == bookingId &&
                                           bp.Booking.member_id == memberId.Value);

            if (bookingPromo == null) return NotFound();

            // Can only remove if booking not paid
            if (bookingPromo.Booking.PaymentStatus == "Paid")
            {
                TempData["Error"] = "Cannot remove promo from paid booking";
                return RedirectToAction("Details", "BOOKINGs", new { id = bookingId });
            }

            // Restore original total
            var booking = bookingPromo.Booking;
            booking.TotalAmount += bookingPromo.DiscountedAmount;

            _context.BOOKING_PROMO.Remove(bookingPromo);
            _context.Update(booking);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Promo code removed successfully";
            return RedirectToAction("Details", "BOOKINGs", new { id = bookingId });
        }

        // =========================
        // VALIDATE - API endpoint to validate promo code during checkout
        // =========================
        [HttpPost]
        public async Task<IActionResult> Validate(string code, int eventId, decimal totalAmount)
        {
            var promo = await _context.PROMO_CODE
                .FirstOrDefaultAsync(p => p.code == code
                    && p.isActive == 'Y'
                    && p.startDate <= DateTime.Now
                    && p.endDate >= DateTime.Now);

            if (promo == null)
            {
                return Json(new { valid = false, message = "Invalid or expired promo code" });
            }

            // Calculate discount
            decimal discountAmount = 0;
            if (promo.DiscountType == "Percentage")
            {
                discountAmount = totalAmount * (promo.DiscountValue / 100);
            }
            else
            {
                discountAmount = promo.DiscountValue;
            }

            discountAmount = Math.Min(discountAmount, totalAmount);

            return Json(new
            {
                valid = true,
                discountAmount = discountAmount,
                promoId = promo.PromoCodeID,
                message = $"Promo applied! You save {discountAmount:C}"
            });
        }

        // =========================
        // REPORT - Summary report by promo
        // =========================
        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Report(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.BOOKING_PROMO
                .Include(bp => bp.PromoCode)
                .Include(bp => bp.Booking)
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(bp => bp.AppliedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(bp => bp.AppliedAt < toDate.Value.AddDays(1));

            // If organizer, filter by their events
            if (!IsAdmin() && IsOrganizer())
            {
                var memberId = await GetCurrentMemberIdAsync();
                if (memberId == null) return Forbid();

                var organizerEvents = await _context.EVENT
                    .Where(e => e.createdByUserID == memberId.Value)
                    .Select(e => e.eventID)
                    .ToListAsync();

                query = query.Where(bp => organizerEvents.Contains(bp.Booking.EventID));
            }

            var report = await query
                .GroupBy(bp => bp.PromoCode.code)
                .Select(g => new PromoReportVM
                {
                    PromoCode = g.Key,
                    TimesUsed = g.Count(),
                    TotalDiscount = g.Sum(bp => bp.DiscountedAmount),
                    TotalBookingValue = g.Sum(bp => bp.Booking.TotalAmount + bp.DiscountedAmount),
                    AverageDiscount = g.Average(bp => bp.DiscountedAmount),
                    FirstUsed = g.Min(bp => bp.AppliedAt),
                    LastUsed = g.Max(bp => bp.AppliedAt)
                })
                .OrderByDescending(r => r.TimesUsed)
                .ToListAsync();

            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.TotalPromosUsed = report.Sum(r => r.TimesUsed);
            ViewBag.TotalDiscountGiven = report.Sum(r => r.TotalDiscount);

            return View(report);
        }
    }

    // ViewModel for promo report
    public class PromoReportVM
    {
        public string PromoCode { get; set; } = string.Empty;
        public int TimesUsed { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalBookingValue { get; set; }
        public decimal AverageDiscount { get; set; }
        public DateTime FirstUsed { get; set; }
        public DateTime LastUsed { get; set; }
    }
}