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
    public class ATTENDANCEsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ATTENDANCEsController(ApplicationDbContext context)
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

        // Search:
        // Admin/Organizer can search all
        // Members can only see their own
        public async Task<IActionResult> Search(string mode, int? attendanceId, int? eventId, int? member_id, int? ticketId, string status)
        {
            var query = _context.Set<ATTENDANCE>().AsQueryable();

            var isAdminOrOrg = await CurrentUserHasAnyRoleAsync("Admin", "Organizer");
            if (!isAdminOrOrg)
            {
                var myMemberId = await GetCurrentMemberIdAsync();
                if (myMemberId == null) return Forbid();
                query = query.Where(a => a.member_id == myMemberId.Value);
            }

            if (mode == "AttendanceID" && attendanceId.HasValue)
                query = query.Where(a => a.AttendanceID == attendanceId.Value);
            else if (mode == "EventID" && eventId.HasValue)
                query = query.Where(a => a.EventID == eventId.Value);
            else if (mode == "MemberID" && member_id.HasValue)
                query = query.Where(a => a.member_id == member_id.Value);
            else if (mode == "TicketID" && ticketId.HasValue)
                query = query.Where(a => a.TicketID == ticketId.Value);
            else if (mode == "Status" && !string.IsNullOrWhiteSpace(status))
                query = query.Where(a => (a.CheckInStatus ?? "").Contains(status));

            return View("Index", await query.ToListAsync());
        }

        // Index:
        // Admin/Organizer sees all, member sees own
        public async Task<IActionResult> Index()
        {
            var isAdminOrOrg = await CurrentUserHasAnyRoleAsync("Admin", "Organizer");
            if (isAdminOrOrg)
                return View(await _context.ATTENDANCE.ToListAsync());

            var myMemberId = await GetCurrentMemberIdAsync();
            if (myMemberId == null) return Forbid();

            return View(await _context.ATTENDANCE.Where(a => a.member_id == myMemberId.Value).ToListAsync());
        }

        // Details: Admin/Organizer OR owner
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var attendance = await _context.ATTENDANCE.FirstOrDefaultAsync(m => m.AttendanceID == id);
            if (attendance == null) return NotFound();

            var isAdminOrOrg = await CurrentUserHasAnyRoleAsync("Admin", "Organizer");
            if (!isAdminOrOrg)
            {
                var myMemberId = await GetCurrentMemberIdAsync();
                if (myMemberId == null || attendance.member_id != myMemberId.Value) return Forbid();
            }

            return View(attendance);
        }

        // Create/Edit/Delete: AdminOrOrganizer only
        [Authorize(Policy = "AdminOrOrganizer")]
        public IActionResult Create() => View();

        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AttendanceID,EventID,member_id,TicketID,CheckedInAt,CheckInStatus")] ATTENDANCE attendance)
        {
            if (ModelState.IsValid)
            {
                _context.Add(attendance);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(attendance);
        }

        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var attendance = await _context.ATTENDANCE.FindAsync(id);
            if (attendance == null) return NotFound();

            return View(attendance);
        }

        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ATTENDANCE attendance)
        {
            if (id != attendance.AttendanceID) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(attendance);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(attendance);
        }

        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var attendance = await _context.ATTENDANCE.FirstOrDefaultAsync(m => m.AttendanceID == id);
            if (attendance == null) return NotFound();

            return View(attendance);
        }

        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var attendance = await _context.ATTENDANCE.FindAsync(id);
            if (attendance != null) _context.ATTENDANCE.Remove(attendance);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
