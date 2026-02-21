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
    public class WAITING_LISTController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WAITING_LISTController(ApplicationDbContext context)
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

        private async Task<bool> CurrentUserHasAnyRoleAsync(params string[] roleNames)
        {
            var memberId = await GetCurrentMemberIdAsync();
            if (memberId == null) return false;

            return await (from ur in _context.USER_ROLE
                          join r in _context.Role on ur.roleID equals r.RoleId
                          where ur.member_id == memberId.Value && roleNames.Contains(r.rolename)
                          select ur.UserRoleID).AnyAsync();
        }

        // =========================
        // JOIN - Add to waiting list
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int eventId)
        {
            try
            {
                var memberId = await GetCurrentMemberIdAsync();
                if (memberId == null)
                {
                    TempData["Error"] = "Please login to join waiting list.";
                    return RedirectToAction("Login", "Account");
                }

                var ev = await _context.EVENT.FindAsync(eventId);
                if (ev == null)
                {
                    TempData["Error"] = "Event not found.";
                    return RedirectToAction("Index", "EVENTs");
                }

                var exists = await _context.WAITING_LIST
                    .AnyAsync(w => w.EventID == eventId
                        && w.member_id == memberId.Value
                        && w.Status == "Pending");

                if (exists)
                {
                    TempData["Info"] = "You are already in the waiting list.";
                    return RedirectToAction("Details", "EVENTs", new { id = eventId });
                }

                var waiting = new WAITING_LIST
                {
                    EventID = eventId,
                    member_id = memberId.Value,
                    AddedAt = DateTime.Now,
                    Status = "Pending"
                };

                _context.WAITING_LIST.Add(waiting);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Added to waiting list.";
                return RedirectToAction("Details", "EVENTs", new { id = eventId });
            }
            catch
            {
                TempData["Error"] = "Error joining waiting list.";
                return RedirectToAction("Details", "EVENTs", new { id = eventId });
            }
        }

        // =========================
        // SEARCH
        // =========================
        public async Task<IActionResult> Search(
            string mode,
            int? waitingListId,
            int? eventId,
            int? member_id,
            string status,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _context.WAITING_LIST.AsQueryable();

            var isAdminOrOrg = await CurrentUserHasAnyRoleAsync("Admin", "Organizer");
            if (!isAdminOrOrg)
            {
                var myId = await GetCurrentMemberIdAsync();
                if (myId == null) return Forbid();
                query = query.Where(w => w.member_id == myId.Value);
            }

            if (mode == "WaitingListID" && waitingListId.HasValue)
                query = query.Where(w => w.WaitingListID == waitingListId.Value);
            else if (mode == "EventID" && eventId.HasValue)
                query = query.Where(w => w.EventID == eventId.Value);
            else if (mode == "MemberID" && member_id.HasValue)
                query = query.Where(w => w.member_id == member_id.Value);
            else if (mode == "Status" && !string.IsNullOrWhiteSpace(status))
                query = query.Where(w => w.Status == status);
            else if (mode == "DateRange")
            {
                if (fromDate.HasValue) query = query.Where(w => w.AddedAt >= fromDate.Value);
                if (toDate.HasValue) query = query.Where(w => w.AddedAt < toDate.Value.AddDays(1));
            }
            else if (mode == "Advanced")
            {
                if (waitingListId.HasValue) query = query.Where(w => w.WaitingListID == waitingListId.Value);
                if (eventId.HasValue) query = query.Where(w => w.EventID == eventId.Value);
                if (member_id.HasValue) query = query.Where(w => w.member_id == member_id.Value);
                if (!string.IsNullOrWhiteSpace(status)) query = query.Where(w => w.Status == status);
                if (fromDate.HasValue) query = query.Where(w => w.AddedAt >= fromDate.Value);
                if (toDate.HasValue) query = query.Where(w => w.AddedAt < toDate.Value.AddDays(1));
            }

            return View("Index", await query.OrderByDescending(w => w.AddedAt).ToListAsync());
        }

        // =========================
        // INDEX
        // =========================
        public async Task<IActionResult> Index()
        {
            var isAdminOrOrg = await CurrentUserHasAnyRoleAsync("Admin", "Organizer");
            if (isAdminOrOrg)
                return View(await _context.WAITING_LIST.OrderByDescending(w => w.AddedAt).ToListAsync());

            var myId = await GetCurrentMemberIdAsync();
            if (myId == null) return Forbid();

            return View(await _context.WAITING_LIST
                .Where(w => w.member_id == myId.Value)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync());
        }

        // =========================
        // DETAILS
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var waiting = await _context.WAITING_LIST.FirstOrDefaultAsync(m => m.WaitingListID == id);
            if (waiting == null) return NotFound();

            var isAdminOrOrg = await CurrentUserHasAnyRoleAsync("Admin", "Organizer");
            if (!isAdminOrOrg)
            {
                var myId = await GetCurrentMemberIdAsync();
                if (myId == null || waiting.member_id != myId.Value) return Forbid();
            }

            return View(waiting);
        }

        // =========================
        // CREATE (GET)
        // =========================
        public IActionResult Create() => View();

        // =========================
        // CREATE (POST)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WAITING_LIST waiting)
        {
            var myId = await GetCurrentMemberIdAsync();
            if (myId == null) return Forbid();

            waiting.member_id = myId.Value;
            waiting.AddedAt = waiting.AddedAt == default ? DateTime.Now : waiting.AddedAt;
            waiting.Status = string.IsNullOrEmpty(waiting.Status) ? "Pending" : waiting.Status;

            if (ModelState.IsValid)
            {
                _context.Add(waiting);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(waiting);
        }

        // =========================
        // EDIT (GET) - Admin/Organizer only
        // =========================
        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var waiting = await _context.WAITING_LIST.FindAsync(id);
            if (waiting == null) return NotFound();

            return View(waiting);
        }

        // =========================
        // EDIT (POST) - Admin/Organizer only
        // =========================
        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WAITING_LIST waiting)
        {
            if (id != waiting.WaitingListID) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(waiting);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(waiting);
        }

        // =========================
        // DELETE (GET) - Admin/Organizer only
        // =========================
        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var waiting = await _context.WAITING_LIST.FirstOrDefaultAsync(m => m.WaitingListID == id);
            if (waiting == null) return NotFound();

            return View(waiting);
        }

        // =========================
        // DELETE (POST) - Admin/Organizer only
        // =========================
        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var waiting = await _context.WAITING_LIST.FindAsync(id);
            if (waiting != null)
                _context.WAITING_LIST.Remove(waiting);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}