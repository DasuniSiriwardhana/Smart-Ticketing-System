using System;
using System.Linq;
using System.Security.Claims;
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
        private readonly IAuthorizationService _authorizationService;

        public BOOKING_PROMOController(ApplicationDbContext context, IAuthorizationService authorizationService)
        {
            _context = context;
            _authorizationService = authorizationService;
        }

        // ================= HELPERS =================

        private string? GetIdentityUserId()
        {
            return User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private async Task<int?> GetCurrentMemberIdAsync()
        {
            var identityUserId = GetIdentityUserId();
            if (string.IsNullOrWhiteSpace(identityUserId))
                return null;

            return await _context.USER
                .Where(u => u.IdentityUserId == identityUserId)
                .Select(u => (int?)u.member_id)
                .FirstOrDefaultAsync();
        }

        private async Task<bool> IsAdminAsync()
        {
            // Uses your policy + handler (YOUR Role + USER_ROLE tables)
            var result = await _authorizationService.AuthorizeAsync(User, "AdminOnly");
            return result.Succeeded;
        }

        private async Task<bool> IsOwnerOfPromoAsync(int bookingPromoId)
        {
            var memberId = await GetCurrentMemberIdAsync();
            if (!memberId.HasValue) return false;

            return await _context.BOOKING_PROMO
                .Join(_context.BOOKING,
                      bp => bp.BookingID,
                      b => b.BookingID,
                      (bp, b) => new { bp.BookingPromoID, b.member_id })
                .AnyAsync(x => x.BookingPromoID == bookingPromoId && x.member_id == memberId.Value);
        }

        private async Task<bool> BookingBelongsToCurrentUserAsync(int bookingId)
        {
            var memberId = await GetCurrentMemberIdAsync();
            if (!memberId.HasValue) return false;

            return await _context.BOOKING
                .AnyAsync(b => b.BookingID == bookingId && b.member_id == memberId.Value);
        }

        // ================= SEARCH =================

        public async Task<IActionResult> Search(
            string mode,
            int? bookingPromoId,
            int? bookingId,
            int? bookingCodeId,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var isAdmin = await IsAdminAsync();

            var query = _context.BOOKING_PROMO.AsQueryable();

            // If not admin -> only see promos for your own bookings
            if (!isAdmin)
            {
                var memberId = await GetCurrentMemberIdAsync();
                if (!memberId.HasValue) return Forbid();

                query = query.Join(_context.BOOKING,
                    bp => bp.BookingID,
                    b => b.BookingID,
                    (bp, b) => new { bp, b })
                    .Where(x => x.b.member_id == memberId.Value)
                    .Select(x => x.bp);
            }

            if (mode == "BookingPromoID" && bookingPromoId.HasValue)
                query = query.Where(x => x.BookingPromoID == bookingPromoId.Value);

            else if (mode == "BookingID" && bookingId.HasValue)
                query = query.Where(x => x.BookingID == bookingId.Value);

            else if (mode == "BookingCodeID" && bookingCodeId.HasValue)
                query = query.Where(x => x.BookingCodeID == bookingCodeId.Value);

            else if (mode == "DateRange")
            {
                if (fromDate.HasValue)
                    query = query.Where(x => x.AppliedAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(x => x.AppliedAt < toDate.Value.AddDays(1));
            }

            else if (mode == "Advanced")
            {
                if (bookingPromoId.HasValue)
                    query = query.Where(x => x.BookingPromoID == bookingPromoId.Value);

                if (bookingId.HasValue)
                    query = query.Where(x => x.BookingID == bookingId.Value);

                if (bookingCodeId.HasValue)
                    query = query.Where(x => x.BookingCodeID == bookingCodeId.Value);

                if (fromDate.HasValue)
                    query = query.Where(x => x.AppliedAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(x => x.AppliedAt < toDate.Value.AddDays(1));
            }

            return View("Index", await query.ToListAsync());
        }

        // ================= INDEX =================

        public async Task<IActionResult> Index()
        {
            var isAdmin = await IsAdminAsync();
            var query = _context.BOOKING_PROMO.AsQueryable();

            if (!isAdmin)
            {
                var memberId = await GetCurrentMemberIdAsync();
                if (!memberId.HasValue) return Forbid();

                query = query.Join(_context.BOOKING,
                    bp => bp.BookingID,
                    b => b.BookingID,
                    (bp, b) => new { bp, b })
                    .Where(x => x.b.member_id == memberId.Value)
                    .Select(x => x.bp);
            }

            return View(await query.ToListAsync());
        }

       

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var isAdmin = await IsAdminAsync();
            if (!isAdmin && !await IsOwnerOfPromoAsync(id.Value))
                return Forbid();

            var promo = await _context.BOOKING_PROMO
                .FirstOrDefaultAsync(m => m.BookingPromoID == id);

            if (promo == null) return NotFound();

            return View(promo);
        }

    

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingPromoID,BookingID,BookingCodeID,DiscountedAmount,AppliedAt")] BOOKING_PROMO promo)
        {
            var isAdmin = await IsAdminAsync();

            // Non-admin can only create promo for THEIR booking
            if (!isAdmin)
            {
                var ok = await BookingBelongsToCurrentUserAsync(promo.BookingID);
                if (!ok) return Forbid();
            }

            if (ModelState.IsValid)
            {
                _context.Add(promo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(promo);
        }

        

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var isAdmin = await IsAdminAsync();
            if (!isAdmin && !await IsOwnerOfPromoAsync(id.Value))
                return Forbid();

            var promo = await _context.BOOKING_PROMO.FindAsync(id);
            if (promo == null) return NotFound();

            return View(promo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingPromoID,BookingID,BookingCodeID,DiscountedAmount,AppliedAt")] BOOKING_PROMO promo)
        {
            if (id != promo.BookingPromoID) return NotFound();

            var isAdmin = await IsAdminAsync();
            if (!isAdmin && !await IsOwnerOfPromoAsync(id))
                return Forbid();

            // Non-admin cannot change BookingID to someone else's booking
            if (!isAdmin)
            {
                var ok = await BookingBelongsToCurrentUserAsync(promo.BookingID);
                if (!ok) return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(promo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    var exists = await _context.BOOKING_PROMO.AnyAsync(e => e.BookingPromoID == promo.BookingPromoID);
                    if (!exists) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(promo);
        }

       
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var isAdmin = await IsAdminAsync();
            if (!isAdmin && !await IsOwnerOfPromoAsync(id.Value))
                return Forbid();

            var promo = await _context.BOOKING_PROMO
                .FirstOrDefaultAsync(m => m.BookingPromoID == id);

            if (promo == null) return NotFound();

            return View(promo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var isAdmin = await IsAdminAsync();
            if (!isAdmin && !await IsOwnerOfPromoAsync(id))
                return Forbid();

            var promo = await _context.BOOKING_PROMO.FindAsync(id);
            if (promo != null)
            {
                _context.BOOKING_PROMO.Remove(promo);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
