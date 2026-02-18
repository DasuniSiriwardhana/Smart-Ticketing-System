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
    public class REVIEW_REPORTController : Controller
    {
        private readonly ApplicationDbContext _context;

        public REVIEW_REPORTController(ApplicationDbContext context)
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
            int? reportId,
            int? reviewId,
            int? reportedByUserId,
            string reportReason,
            string reportDetail,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _context.Set<REVIEW_REPORT>().AsQueryable();

            var isAdminOrOrg = await CurrentUserHasAnyRoleAsync("Admin", "Organizer");
            if (!isAdminOrOrg)
            {
                var myId = await GetCurrentMemberIdAsync();
                if (myId == null) return Forbid();
                query = query.Where(r => r.ReportedByUserID == myId.Value);
            }

            if (mode == "ReportID" && reportId.HasValue)
                query = query.Where(r => r.RportID == reportId.Value);
            else if (mode == "ReviewID" && reviewId.HasValue)
                query = query.Where(r => r.ReviewID == reviewId.Value);
            else if (mode == "ReportedByUserID" && reportedByUserId.HasValue)
                query = query.Where(r => r.ReportedByUserID == reportedByUserId.Value);
            else if (mode == "ReportReason" && !string.IsNullOrWhiteSpace(reportReason))
                query = query.Where(r => (r.ReportReason ?? "").Contains(reportReason));
            else if (mode == "ReportDetail" && !string.IsNullOrWhiteSpace(reportDetail))
                query = query.Where(r => (r.ReportDetail ?? "").Contains(reportDetail));
            else if (mode == "DateRange")
            {
                if (fromDate.HasValue) query = query.Where(r => r.ReportedAt >= fromDate.Value);
                if (toDate.HasValue) query = query.Where(r => r.ReportedAt < toDate.Value.AddDays(1));
            }
            else if (mode == "Advanced")
            {
                if (reportId.HasValue) query = query.Where(r => r.RportID == reportId.Value);
                if (reviewId.HasValue) query = query.Where(r => r.ReviewID == reviewId.Value);
                if (reportedByUserId.HasValue) query = query.Where(r => r.ReportedByUserID == reportedByUserId.Value);

                if (!string.IsNullOrWhiteSpace(reportReason))
                    query = query.Where(r => (r.ReportReason ?? "").Contains(reportReason));

                if (!string.IsNullOrWhiteSpace(reportDetail))
                    query = query.Where(r => (r.ReportDetail ?? "").Contains(reportDetail));

                if (fromDate.HasValue) query = query.Where(r => r.ReportedAt >= fromDate.Value);
                if (toDate.HasValue) query = query.Where(r => r.ReportedAt < toDate.Value.AddDays(1));
            }

            return View("Index", await query
                .OrderByDescending(r => r.ReportedAt)
                .ToListAsync());
        }

        // INDEX: Admin/Organizer all, Member only own
        public async Task<IActionResult> Index()
        {
            var isAdminOrOrg = await CurrentUserHasAnyRoleAsync("Admin", "Organizer");
            if (isAdminOrOrg)
                return View(await _context.REVIEW_REPORT.OrderByDescending(r => r.ReportedAt).ToListAsync());

            var myId = await GetCurrentMemberIdAsync();
            if (myId == null) return Forbid();

            return View(await _context.REVIEW_REPORT
                .Where(r => r.ReportedByUserID == myId.Value)
                .OrderByDescending(r => r.ReportedAt)
                .ToListAsync());
        }

        // DETAILS: Admin/Organizer OR owner
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var report = await _context.REVIEW_REPORT.FirstOrDefaultAsync(m => m.RportID == id);
            if (report == null) return NotFound();

            var isAdminOrOrg = await CurrentUserHasAnyRoleAsync("Admin", "Organizer");
            if (!isAdminOrOrg)
            {
                var myId = await GetCurrentMemberIdAsync();
                if (myId == null || report.ReportedByUserID != myId.Value) return Forbid();
            }

            return View(report);
        }

        // CREATE: Member can create report (but we force ReportedByUserID = current user)
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RportID,ReviewID,ReportedByUserID,ReportReason,ReportDetail,ReportedAt")] REVIEW_REPORT rEVIEW_REPORT)
        {
            var myId = await GetCurrentMemberIdAsync();
            if (myId == null) return Forbid();

            // force correct ownership + timestamp
            rEVIEW_REPORT.ReportedByUserID = myId.Value;
            rEVIEW_REPORT.ReportedAt = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Add(rEVIEW_REPORT);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(rEVIEW_REPORT);
        }

        // EDIT/DELETE: AdminOrOrganizer only
        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var report = await _context.REVIEW_REPORT.FindAsync(id);
            if (report == null) return NotFound();

            return View(report);
        }

        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RportID,ReviewID,ReportedByUserID,ReportReason,ReportDetail,ReportedAt")] REVIEW_REPORT rEVIEW_REPORT)
        {
            if (id != rEVIEW_REPORT.RportID) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(rEVIEW_REPORT);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(rEVIEW_REPORT);
        }

        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var report = await _context.REVIEW_REPORT.FirstOrDefaultAsync(m => m.RportID == id);
            if (report == null) return NotFound();

            return View(report);
        }

        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var report = await _context.REVIEW_REPORT.FindAsync(id);
            if (report != null)
                _context.REVIEW_REPORT.Remove(report);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
