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
    public class REVIEWsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public REVIEWsController(ApplicationDbContext context)
        {
            _context = context;
        }

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

        // SEARCH: Admin/Organizer all, Member only own
        public async Task<IActionResult> Search(
            string mode,
            int? reviewId,
            int? eventId,
            int? member_id,
            int? minRating,
            int? maxRating,
            char? isVerifiedAttendee,
            string reviewStatus,
            string commentsText,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _context.Set<REVIEW>().AsQueryable();

            var isAdminOrOrg = await CurrentUserHasAnyRoleAsync("Admin", "Organizer");
            if (!isAdminOrOrg)
            {
                var myId = await GetCurrentMemberIdAsync();
                if (myId == null) return Forbid();
                query = query.Where(r => r.member_id == myId.Value);
            }

            if (mode == "ReviewID" && reviewId.HasValue)
                query = query.Where(r => r.ReviewID == reviewId.Value);
            else if (mode == "EventID" && eventId.HasValue)
                query = query.Where(r => r.eventID == eventId.Value);
            else if (mode == "UserID" && member_id.HasValue)
                query = query.Where(r => r.member_id == member_id.Value);
            else if (mode == "RatingRange")
            {
                if (minRating.HasValue) query = query.Where(r => r.Ratings >= minRating.Value);
                if (maxRating.HasValue) query = query.Where(r => r.Ratings <= maxRating.Value);
            }
            else if (mode == "Verified" && isVerifiedAttendee.HasValue)
                query = query.Where(r => r.isVerifiedAttendee == isVerifiedAttendee.Value);
            else if (mode == "ReviewStatus" && !string.IsNullOrWhiteSpace(reviewStatus))
                query = query.Where(r => r.ReviewStatus == reviewStatus);
            else if (mode == "Comments" && !string.IsNullOrWhiteSpace(commentsText))
                query = query.Where(r => (r.Comments ?? "").Contains(commentsText));
            else if (mode == "DateRange")
            {
                if (fromDate.HasValue) query = query.Where(r => r.createdAt >= fromDate.Value);
                if (toDate.HasValue) query = query.Where(r => r.createdAt < toDate.Value.AddDays(1));
            }
            else if (mode == "Advanced")
            {
                if (reviewId.HasValue) query = query.Where(r => r.ReviewID == reviewId.Value);
                if (eventId.HasValue) query = query.Where(r => r.eventID == eventId.Value);
                if (member_id.HasValue) query = query.Where(r => r.member_id == member_id.Value);

                if (minRating.HasValue) query = query.Where(r => r.Ratings >= minRating.Value);
                if (maxRating.HasValue) query = query.Where(r => r.Ratings <= maxRating.Value);

                if (isVerifiedAttendee.HasValue) query = query.Where(r => r.isVerifiedAttendee == isVerifiedAttendee.Value);

                if (!string.IsNullOrWhiteSpace(reviewStatus)) query = query.Where(r => r.ReviewStatus == reviewStatus);
                if (!string.IsNullOrWhiteSpace(commentsText)) query = query.Where(r => (r.Comments ?? "").Contains(commentsText));

                if (fromDate.HasValue) query = query.Where(r => r.createdAt >= fromDate.Value);
                if (toDate.HasValue) query = query.Where(r => r.createdAt < toDate.Value.AddDays(1));
            }

            return View("Index", await query.OrderByDescending(r => r.createdAt).ToListAsync());
        }

        // INDEX: Admin/Organizer all, Member only own
        public async Task<IActionResult> Index()
        {
            var isAdminOrOrg = await CurrentUserHasAnyRoleAsync("Admin", "Organizer");
            if (isAdminOrOrg)
                return View(await _context.REVIEW.OrderByDescending(r => r.createdAt).ToListAsync());

            var myId = await GetCurrentMemberIdAsync();
            if (myId == null) return Forbid();

            return View(await _context.REVIEW
                .Where(r => r.member_id == myId.Value)
                .OrderByDescending(r => r.createdAt)
                .ToListAsync());
        }

        // DETAILS: Admin/Organizer OR owner
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var review = await _context.REVIEW.FirstOrDefaultAsync(m => m.ReviewID == id);
            if (review == null) return NotFound();

            var isAdminOrOrg = await CurrentUserHasAnyRoleAsync("Admin", "Organizer");
            if (!isAdminOrOrg)
            {
                var myId = await GetCurrentMemberIdAsync();
                if (myId == null || review.member_id != myId.Value) return Forbid();
            }

            return View(review);
        }

        // CREATE: Member creates own review (force member_id)
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ReviewID,eventID,member_id,Ratings,Comments,isVerifiedAttendee,ReviewStatus")] REVIEW review)
        {
            var myId = await GetCurrentMemberIdAsync();
            if (myId == null) return Forbid();

            review.member_id = myId.Value;
            review.createdAt = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Add(review);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(review);
        }

        // EDIT: owner OR Admin/Organizer
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var review = await _context.REVIEW.FindAsync(id);
            if (review == null) return NotFound();

            var isAdminOrOrg = await CurrentUserHasAnyRoleAsync("Admin", "Organizer");
            if (!isAdminOrOrg)
            {
                var myId = await GetCurrentMemberIdAsync();
                if (myId == null || review.member_id != myId.Value) return Forbid();
            }

            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReviewID,eventID,member_id,Ratings,Comments,isVerifiedAttendee,ReviewStatus,createdAt")] REVIEW review)
        {
            if (id != review.ReviewID) return NotFound();

            var existing = await _context.REVIEW.AsNoTracking().FirstOrDefaultAsync(r => r.ReviewID == id);
            if (existing == null) return NotFound();

            var isAdminOrOrg = await CurrentUserHasAnyRoleAsync("Admin", "Organizer");
            if (!isAdminOrOrg)
            {
                var myId = await GetCurrentMemberIdAsync();
                if (myId == null || existing.member_id != myId.Value) return Forbid();

                // prevent changing ownership
                review.member_id = existing.member_id;
            }

            if (ModelState.IsValid)
            {
                _context.Update(review);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(review);
        }

        // DELETE: owner OR Admin/Organizer
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var review = await _context.REVIEW.FirstOrDefaultAsync(m => m.ReviewID == id);
            if (review == null) return NotFound();

            var isAdminOrOrg = await CurrentUserHasAnyRoleAsync("Admin", "Organizer");
            if (!isAdminOrOrg)
            {
                var myId = await GetCurrentMemberIdAsync();
                if (myId == null || review.member_id != myId.Value) return Forbid();
            }

            return View(review);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.REVIEW.FindAsync(id);
            if (review == null) return NotFound();

            var isAdminOrOrg = await CurrentUserHasAnyRoleAsync("Admin", "Organizer");
            if (!isAdminOrOrg)
            {
                var myId = await GetCurrentMemberIdAsync();
                if (myId == null || review.member_id != myId.Value) return Forbid();
            }

            _context.REVIEW.Remove(review);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
