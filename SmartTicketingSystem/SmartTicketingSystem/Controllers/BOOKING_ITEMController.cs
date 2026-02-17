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
    public class BOOKING_ITEMController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BOOKING_ITEMController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= HELPER METHODS =================

        private string? GetIdentityUserId()
        {
            return User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        private async Task<int?> GetCurrentMemberIdAsync()
        {
            var identityUserId = GetIdentityUserId();
            if (string.IsNullOrEmpty(identityUserId))
                return null;

            return await _context.USER
                .Where(u => u.IdentityUserId == identityUserId)
                .Select(u => (int?)u.member_id)
                .FirstOrDefaultAsync();
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        private async Task<bool> IsOwnerAsync(int bookingItemId)
        {
            var memberId = await GetCurrentMemberIdAsync();
            if (!memberId.HasValue)
                return false;

            return await _context.BOOKING_ITEM
                .Join(_context.BOOKING,
                      bi => bi.BookingID,
                      b => b.BookingID,
                      (bi, b) => new { bi.BookingItemID, b.member_id })
                .AnyAsync(x => x.BookingItemID == bookingItemId &&
                               x.member_id == memberId.Value);
        }

        // ================= SEARCH =================

        public async Task<IActionResult> Search(
            string mode,
            int? bookingItemId,
            int? bookingId,
            int? ticketTypeId,
            int? quantity)
        {
            var query = _context.BOOKING_ITEM.AsQueryable();

            if (!IsAdmin())
            {
                var memberId = await GetCurrentMemberIdAsync();
                if (!memberId.HasValue)
                    return Forbid();

                query = query.Join(_context.BOOKING,
                    bi => bi.BookingID,
                    b => b.BookingID,
                    (bi, b) => new { bi, b })
                    .Where(x => x.b.member_id == memberId.Value)
                    .Select(x => x.bi);
            }

            if (mode == "BookingItemID" && bookingItemId.HasValue)
                query = query.Where(x => x.BookingItemID == bookingItemId.Value);

            else if (mode == "BookingID" && bookingId.HasValue)
                query = query.Where(x => x.BookingID == bookingId.Value);

            else if (mode == "TicketTypeID" && ticketTypeId.HasValue)
                query = query.Where(x => x.TicketTypeID == ticketTypeId.Value);

            else if (mode == "Quantity" && quantity.HasValue)
                query = query.Where(x => x.Quantity == quantity.Value);

            return View("Index", await query.ToListAsync());
        }

        // ================= INDEX =================

        public async Task<IActionResult> Index()
        {
            var query = _context.BOOKING_ITEM.AsQueryable();

            if (!IsAdmin())
            {
                var memberId = await GetCurrentMemberIdAsync();
                if (!memberId.HasValue)
                    return Forbid();

                query = query.Join(_context.BOOKING,
                    bi => bi.BookingID,
                    b => b.BookingID,
                    (bi, b) => new { bi, b })
                    .Where(x => x.b.member_id == memberId.Value)
                    .Select(x => x.bi);
            }

            return View(await query.ToListAsync());
        }

        // ================= DETAILS =================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            if (!IsAdmin() && !await IsOwnerAsync(id.Value))
                return Forbid();

            var item = await _context.BOOKING_ITEM
                .FirstOrDefaultAsync(m => m.BookingItemID == id);

            if (item == null)
                return NotFound();

            return View(item);
        }

        // ================= CREATE =================

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("BookingItemID,BookingID,TicketTypeID,Quantity,UnitPrice,LineTotal")] BOOKING_ITEM item)
        {
            if (!IsAdmin())
            {
                var memberId = await GetCurrentMemberIdAsync();
                var booking = await _context.BOOKING
                    .FirstOrDefaultAsync(b => b.BookingID == item.BookingID);

                if (booking == null || booking.member_id != memberId)
                    return Forbid();
            }

            if (ModelState.IsValid)
            {
                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(item);
        }

        // ================= EDIT =================

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            if (!IsAdmin() && !await IsOwnerAsync(id.Value))
                return Forbid();

            var item = await _context.BOOKING_ITEM.FindAsync(id);
            if (item == null)
                return NotFound();

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("BookingItemID,BookingID,TicketTypeID,Quantity,UnitPrice,LineTotal")] BOOKING_ITEM item)
        {
            if (id != item.BookingItemID)
                return NotFound();

            if (!IsAdmin() && !await IsOwnerAsync(id))
                return Forbid();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(item);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.BOOKING_ITEM.Any(e => e.BookingItemID == id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(item);
        }

        // ================= DELETE =================

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            if (!IsAdmin() && !await IsOwnerAsync(id.Value))
                return Forbid();

            var item = await _context.BOOKING_ITEM
                .FirstOrDefaultAsync(m => m.BookingItemID == id);

            if (item == null)
                return NotFound();

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin() && !await IsOwnerAsync(id))
                return Forbid();

            var item = await _context.BOOKING_ITEM.FindAsync(id);

            if (item != null)
                _context.BOOKING_ITEM.Remove(item);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
