using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTicketingSystem.Data;
using SmartTicketingSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SmartTicketingSystem.Controllers
{
    [Authorize(Policy = "AdminOrOrganizer")]
    public class PROMO_CODEController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PROMO_CODEController(ApplicationDbContext context)
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

        // Check if user can manage promos for this event
        private async Task<bool> CanManageEventPromos(int eventId)
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

        // =========================
        // SEARCH
        // =========================
        public async Task<IActionResult> Search(
            string mode,
            int? promoCodeId,
            string code,
            string discountType,
            decimal? minDiscount,
            decimal? maxDiscount,
            char? isActive,
            DateTime? fromStartDate,
            DateTime? toEndDate,
            DateTime? fromCreatedDate,
            DateTime? toCreatedDate,
            int? eventId = null)
        {
            var query = _context.PROMO_CODE.AsQueryable();

            // Filter by event if specified
            if (eventId.HasValue)
            {
                if (!await CanManageEventPromos(eventId.Value))
                    return Forbid();

                query = query.Where(p => _context.BOOKING_PROMO
                    .Any(bp => bp.BookingCodeID == p.PromoCodeID &&
                               bp.Booking.EventID == eventId.Value));
            }

            // Organizer filter (non-admin)
            if (!IsAdmin() && IsOrganizer() && !eventId.HasValue)
            {
                var memberId = await GetCurrentMemberIdAsync();
                if (memberId == null) return Forbid();

                var organizerEvents = await _context.EVENT
                    .Where(e => e.createdByUserID == memberId.Value)
                    .Select(e => e.eventID)
                    .ToListAsync();

                query = query.Where(p => _context.BOOKING_PROMO
                    .Any(bp => bp.BookingCodeID == p.PromoCodeID &&
                               organizerEvents.Contains(bp.Booking.EventID)));
            }

            // Search modes
            if (mode == "PromoCodeID" && promoCodeId.HasValue)
                query = query.Where(p => p.PromoCodeID == promoCodeId.Value);

            else if (mode == "Code" && !string.IsNullOrWhiteSpace(code))
                query = query.Where(p => p.code.Contains(code));

            else if (mode == "DiscountType" && !string.IsNullOrWhiteSpace(discountType))
                query = query.Where(p => p.DiscountType == discountType);

            else if (mode == "DiscountRange")
            {
                if (minDiscount.HasValue) query = query.Where(p => p.DiscountValue >= minDiscount.Value);
                if (maxDiscount.HasValue) query = query.Where(p => p.DiscountValue <= maxDiscount.Value);
            }

            else if (mode == "Status" && isActive.HasValue)
                query = query.Where(p => p.isActive == isActive.Value);

            else if (mode == "ValidityRange")
            {
                if (fromStartDate.HasValue) query = query.Where(p => p.startDate >= fromStartDate.Value);
                if (toEndDate.HasValue) query = query.Where(p => p.endDate <= toEndDate.Value);
            }

            else if (mode == "Advanced")
            {
                if (promoCodeId.HasValue) query = query.Where(p => p.PromoCodeID == promoCodeId.Value);
                if (!string.IsNullOrWhiteSpace(code)) query = query.Where(p => p.code.Contains(code));
                if (!string.IsNullOrWhiteSpace(discountType)) query = query.Where(p => p.DiscountType == discountType);
                if (minDiscount.HasValue) query = query.Where(p => p.DiscountValue >= minDiscount.Value);
                if (maxDiscount.HasValue) query = query.Where(p => p.DiscountValue <= maxDiscount.Value);
                if (isActive.HasValue) query = query.Where(p => p.isActive == isActive.Value);
                if (fromStartDate.HasValue) query = query.Where(p => p.startDate >= fromStartDate.Value);
                if (toEndDate.HasValue) query = query.Where(p => p.endDate <= toEndDate.Value);
                if (fromCreatedDate.HasValue) query = query.Where(p => p.createdAt >= fromCreatedDate.Value);
                if (toCreatedDate.HasValue) query = query.Where(p => p.createdAt < toCreatedDate.Value.AddDays(1));
            }

            return View("Index", await query.OrderByDescending(p => p.createdAt).ToListAsync());
        }

        // =========================
        // INDEX
        // =========================
        public async Task<IActionResult> Index(int? eventId = null)
        {
            IQueryable<PROMO_CODE> query = _context.PROMO_CODE;

            // Filter by specific event if requested
            if (eventId.HasValue)
            {
                if (!await CanManageEventPromos(eventId.Value))
                    return Forbid();

                var ev = await _context.EVENT.FindAsync(eventId.Value);
                ViewBag.Event = ev;

                query = query.Where(p => _context.BOOKING_PROMO
                    .Any(bp => bp.BookingCodeID == p.PromoCodeID &&
                               bp.Booking.EventID == eventId.Value));
            }
            // If organizer and no event specified, show promos for their events
            else if (!IsAdmin() && IsOrganizer())
            {
                var memberId = await GetCurrentMemberIdAsync();
                if (memberId == null) return Forbid();

                var organizerEvents = await _context.EVENT
                    .Where(e => e.createdByUserID == memberId.Value)
                    .Select(e => e.eventID)
                    .ToListAsync();

                query = query.Where(p => _context.BOOKING_PROMO
                    .Any(bp => bp.BookingCodeID == p.PromoCodeID &&
                               organizerEvents.Contains(bp.Booking.EventID)));
            }

            var promos = await query
                .OrderByDescending(p => p.createdAt)
                .ToListAsync();

            // Get usage stats for each promo
            var usageStats = new Dictionary<int, (int Count, decimal TotalDiscount)>();
            foreach (var promo in promos)
            {
                var usage = await _context.BOOKING_PROMO
                    .Where(bp => bp.BookingCodeID == promo.PromoCodeID)
                    .SumAsync(bp => (int?)1) ?? 0;

                var discount = await _context.BOOKING_PROMO
                    .Where(bp => bp.BookingCodeID == promo.PromoCodeID)
                    .SumAsync(bp => (decimal?)bp.DiscountedAmount) ?? 0;

                usageStats[promo.PromoCodeID] = (usage, discount);
            }

            ViewBag.UsageStats = usageStats;
            ViewBag.EventId = eventId;

            return View(promos);
        }

        // =========================
        // DETAILS
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var promo = await _context.PROMO_CODE
                .FirstOrDefaultAsync(m => m.PromoCodeID == id);

            if (promo == null) return NotFound();

            // Check if user can view this promo
            if (!IsAdmin())
            {
                var memberId = await GetCurrentMemberIdAsync();
                if (memberId == null) return Forbid();

                var canView = await _context.BOOKING_PROMO
                    .AnyAsync(bp => bp.BookingCodeID == id &&
                                    _context.EVENT.Any(e => e.eventID == bp.Booking.EventID &&
                                                           e.createdByUserID == memberId.Value));

                if (!canView) return Forbid();
            }

            // Get usage details
            var usage = await _context.BOOKING_PROMO
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.Event)
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.User)
                .Where(bp => bp.BookingCodeID == id)
                .OrderByDescending(bp => bp.AppliedAt)
                .ToListAsync();

            ViewBag.Usage = usage;

            return View(promo);
        }

        // =========================
        // CREATE (GET)
        // =========================
        public async Task<IActionResult> Create(int? eventId = null)
        {
            if (eventId.HasValue && !await CanManageEventPromos(eventId.Value))
                return Forbid();

            var promo = new PROMO_CODE
            {
                startDate = DateTime.Now,
                endDate = DateTime.Now.AddMonths(1),
                isActive = 'Y'
            };

            if (eventId.HasValue)
            {
                var ev = await _context.EVENT.FindAsync(eventId.Value);
                ViewBag.Event = ev;
            }

            ViewBag.EventId = eventId;
            ViewBag.DiscountTypes = new SelectList(new[] { "Percentage", "Fixed" });

            return View(promo);
        }

        // =========================
        // CREATE (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PROMO_CODE promo, int? eventId = null)
        {
            if (eventId.HasValue && !await CanManageEventPromos(eventId.Value))
                return Forbid();

            // Validate dates
            if (promo.endDate <= promo.startDate)
            {
                ModelState.AddModelError("endDate", "End date must be after start date");
            }

            // Check if code already exists
            var exists = await _context.PROMO_CODE
                .AnyAsync(p => p.code == promo.code);

            if (exists)
            {
                ModelState.AddModelError("code", "This promo code already exists");
            }

            // Validate discount value
            if (promo.DiscountType == "Percentage" && (promo.DiscountValue <= 0 || promo.DiscountValue > 100))
            {
                ModelState.AddModelError("DiscountValue", "Percentage must be between 1 and 100");
            }
            else if (promo.DiscountType == "Fixed" && promo.DiscountValue <= 0)
            {
                ModelState.AddModelError("DiscountValue", "Fixed amount must be greater than 0");
            }

            if (ModelState.IsValid)
            {
                promo.createdAt = DateTime.Now;
                promo.isActive = 'Y';

                _context.Add(promo);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Promo code '{promo.code}' created successfully!";

                if (eventId.HasValue)
                    return RedirectToAction("ForEvent", "PROMO_CODE", new { eventId = eventId.Value });
                else
                    return RedirectToAction(nameof(Index));
            }

            if (eventId.HasValue)
            {
                var ev = await _context.EVENT.FindAsync(eventId.Value);
                ViewBag.Event = ev;
            }

            ViewBag.EventId = eventId;
            ViewBag.DiscountTypes = new SelectList(new[] { "Percentage", "Fixed" }, promo.DiscountType);

            return View(promo);
        }

        // =========================
        // EDIT (GET)
        // =========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var promo = await _context.PROMO_CODE
                .Include(p => p.BookingPromos)
                .ThenInclude(bp => bp.Booking)
                .FirstOrDefaultAsync(p => p.PromoCodeID == id);

            if (promo == null) return NotFound();

            // Check permission
            if (!IsAdmin())
            {
                var memberId = await GetCurrentMemberIdAsync();
                if (memberId == null) return Forbid();

                var canEdit = promo.BookingPromos.Any(bp =>
                    _context.EVENT.Any(e => e.eventID == bp.Booking.EventID &&
                                           e.createdByUserID == memberId.Value));

                if (!canEdit && promo.BookingPromos.Any())
                    return Forbid();
            }

            // Warn if promo has been used
            if (promo.BookingPromos.Any())
            {
                TempData["Warning"] = "This promo code has been used. Some fields may not be editable.";
            }

            ViewBag.DiscountTypes = new SelectList(new[] { "Percentage", "Fixed" }, promo.DiscountType);

            return View(promo);
        }

        // =========================
        // EDIT (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PROMO_CODE promo)
        {
            if (id != promo.PromoCodeID) return NotFound();

            // Check if user can edit this promo
            var existing = await _context.PROMO_CODE
                .Include(p => p.BookingPromos)
                .FirstOrDefaultAsync(p => p.PromoCodeID == id);

            if (existing == null) return NotFound();

            if (!IsAdmin())
            {
                var memberId = await GetCurrentMemberIdAsync();
                if (memberId == null) return Forbid();

                var canEdit = existing.BookingPromos.Any(bp =>
                    _context.EVENT.Any(e => e.eventID == bp.Booking.EventID &&
                                           e.createdByUserID == memberId.Value));

                if (!canEdit && existing.BookingPromos.Any())
                    return Forbid();
            }

            // Check if code already exists (excluding current)
            var codeExists = await _context.PROMO_CODE
                .AnyAsync(p => p.code == promo.code && p.PromoCodeID != id);

            if (codeExists)
            {
                ModelState.AddModelError("code", "This promo code already exists");
            }

            // Validate dates
            if (promo.endDate <= promo.startDate)
            {
                ModelState.AddModelError("endDate", "End date must be after start date");
            }

            // If promo has been used, prevent changing certain fields
            if (existing.BookingPromos.Any())
            {
                if (promo.DiscountType != existing.DiscountType)
                {
                    ModelState.AddModelError("DiscountType", "Cannot change discount type after promo has been used");
                }

                // Keep original code
                promo.code = existing.code;
            }

            // Validate discount value
            if (promo.DiscountType == "Percentage" && (promo.DiscountValue <= 0 || promo.DiscountValue > 100))
            {
                ModelState.AddModelError("DiscountValue", "Percentage must be between 1 and 100");
            }
            else if (promo.DiscountType == "Fixed" && promo.DiscountValue <= 0)
            {
                ModelState.AddModelError("DiscountValue", "Fixed amount must be greater than 0");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(promo);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Promo code updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.PROMO_CODE.Any(e => e.PromoCodeID == id))
                        return NotFound();
                    throw;
                }
            }

            ViewBag.DiscountTypes = new SelectList(new[] { "Percentage", "Fixed" }, promo.DiscountType);

            return View(promo);
        }

        // =========================
        // FOR EVENT - Show promos for a specific event
        // =========================
        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> ForEvent(int eventId)
        {
            if (!await CanManageEventPromos(eventId))
                return Forbid();

            var ev = await _context.EVENT.FindAsync(eventId);
            if (ev == null) return NotFound();

            // Get all promos that have been used for this event
            var usedPromos = await _context.BOOKING_PROMO
                .Include(bp => bp.PromoCode)
                .Where(bp => bp.Booking.EventID == eventId)
                .Select(bp => bp.PromoCode)
                .Distinct()
                .ToListAsync();

            // Get available promos (created by admin/organizer but not used for this event yet)
            var memberId = await GetCurrentMemberIdAsync();
            IQueryable<PROMO_CODE> availableQuery = _context.PROMO_CODE
                .Where(p => p.isActive == 'Y');

            if (!IsAdmin())
            {
                // Organizers can only see promos they created or that are used in their events
                availableQuery = availableQuery.Where(p =>
                    _context.BOOKING_PROMO.Any(bp => bp.BookingCodeID == p.PromoCodeID &&
                                                     bp.Booking.EventID == eventId) ||
                    p.BookingPromos.Any(bp => _context.EVENT.Any(e => e.eventID == bp.Booking.EventID &&
                                                                     e.createdByUserID == memberId)));
            }

            var availablePromos = await availableQuery
                .Where(p => !usedPromos.Select(up => up.PromoCodeID).Contains(p.PromoCodeID))
                .ToListAsync();

            ViewBag.Event = ev;
            ViewBag.UsedPromos = usedPromos;
            ViewBag.AvailablePromos = availablePromos;

            return View();
        }

        // =========================
        // ASSIGN TO EVENT - Link promo to event
        // =========================
        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignToEvent(int promoId, int eventId)
        {
            if (!await CanManageEventPromos(eventId))
                return Forbid();

            var promo = await _context.PROMO_CODE.FindAsync(promoId);
            if (promo == null) return NotFound();

            // This just records that this promo is now available for this event
            // The actual linking happens when a user uses the promo
            TempData["Success"] = $"Promo code '{promo.code}' is now available for this event";

            return RedirectToAction(nameof(ForEvent), new { eventId });
        }

        // =========================
        // TOGGLE STATUS
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var promo = await _context.PROMO_CODE
                .Include(p => p.BookingPromos)
                .FirstOrDefaultAsync(p => p.PromoCodeID == id);

            if (promo == null) return NotFound();

            // Check permission
            if (!IsAdmin())
            {
                var memberId = await GetCurrentMemberIdAsync();
                if (memberId == null) return Forbid();

                var canManage = promo.BookingPromos.Any(bp =>
                    _context.EVENT.Any(e => e.eventID == bp.Booking.EventID &&
                                           e.createdByUserID == memberId.Value));

                if (!canManage && promo.BookingPromos.Any())
                    return Forbid();
            }

            promo.isActive = promo.isActive == 'Y' ? 'N' : 'Y';
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Promo code '{promo.code}' {(promo.isActive == 'Y' ? "activated" : "deactivated")}";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // DELETE (GET)
        // =========================
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var promo = await _context.PROMO_CODE
                .Include(p => p.BookingPromos)
                .FirstOrDefaultAsync(m => m.PromoCodeID == id);

            if (promo == null) return NotFound();

            if (promo.BookingPromos.Any())
            {
                TempData["Error"] = "Cannot delete promo code that has been used. Deactivate it instead.";
                return RedirectToAction(nameof(Index));
            }

            return View(promo);
        }

        // =========================
        // DELETE (POST)
        // =========================
        [Authorize(Policy = "AdminOnly")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var promo = await _context.PROMO_CODE
                .Include(p => p.BookingPromos)
                .FirstOrDefaultAsync(p => p.PromoCodeID == id);

            if (promo == null) return NotFound();

            if (promo.BookingPromos.Any())
            {
                TempData["Error"] = "Cannot delete promo code that has been used.";
                return RedirectToAction(nameof(Index));
            }

            _context.PROMO_CODE.Remove(promo);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Promo code deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // USAGE REPORT
        // =========================
        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Usage(int? promoId = null, int? eventId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            IQueryable<BOOKING_PROMO> query = _context.BOOKING_PROMO
                .Include(bp => bp.PromoCode)
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.User)
                .Include(bp => bp.Booking)
                    .ThenInclude(b => b.Event)
                .OrderByDescending(bp => bp.AppliedAt);

            // Filter by event
            if (eventId.HasValue)
            {
                if (!await CanManageEventPromos(eventId.Value))
                    return Forbid();

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

            // Filter by date range
            if (fromDate.HasValue)
                query = query.Where(bp => bp.AppliedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(bp => bp.AppliedAt < toDate.Value.AddDays(1));

            // If organizer (not admin), filter by their events
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

            var usage = await query.ToListAsync();

            // Calculate summary
            var summary = new PromoUsageSummaryVM
            {
                TotalUsage = usage.Count,
                TotalDiscount = usage.Sum(bp => bp.DiscountedAmount),
                TotalBookingValue = usage.Sum(bp => bp.Booking.TotalAmount + bp.DiscountedAmount),
                AverageDiscount = usage.Any() ? usage.Average(bp => bp.DiscountedAmount) : 0,
                UniqueUsers = usage.Select(bp => bp.Booking.member_id).Distinct().Count()
            };

            ViewBag.Summary = summary;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.EventId = eventId;
            ViewBag.PromoId = promoId;

            return View(usage);
        }
    }

    // ViewModel for promo usage summary
    public class PromoUsageSummaryVM
    {
        public int TotalUsage { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalBookingValue { get; set; }
        public decimal AverageDiscount { get; set; }
        public int UniqueUsers { get; set; }
    }
}