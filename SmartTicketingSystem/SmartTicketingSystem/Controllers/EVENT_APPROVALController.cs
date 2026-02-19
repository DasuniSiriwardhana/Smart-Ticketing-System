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
    [Authorize(Policy = "AdminOnly")]
    public class EVENT_APPROVALController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EVENT_APPROVALController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Search(string mode, int? approvalId, int? eventId, int? approvedByUserId, int? memberId, char? decision, string decisionNote, System.DateTime? fromDate, System.DateTime? toDate)
        {
            var query = _context.Set<EVENT_APPROVAL>().AsQueryable();

            if (mode == "ApprovalID" && approvalId.HasValue) query = query.Where(x => x.ApprovalID == approvalId.Value);
            else if (mode == "EventID" && eventId.HasValue) query = query.Where(x => x.EventID == eventId.Value);
            else if (mode == "ApprovedByUserID" && approvedByUserId.HasValue) query = query.Where(x => x.ApprovedByUserID == approvedByUserId.Value);
            else if (mode == "MemberID" && memberId.HasValue) query = query.Where(x => x.member_id == memberId.Value);
            else if (mode == "Decision" && decision.HasValue) query = query.Where(x => x.Decision == decision.Value);
            else if (mode == "DecisionNote" && !string.IsNullOrWhiteSpace(decisionNote)) query = query.Where(x => (x.DecisionNote ?? "").Contains(decisionNote));
            else if (mode == "DateRange")
            {
                if (fromDate.HasValue) query = query.Where(x => x.DecisionDateTime >= fromDate.Value);
                if (toDate.HasValue) query = query.Where(x => x.DecisionDateTime < toDate.Value.AddDays(1));
            }

            return View("Index", await query.ToListAsync());
        }

        public async Task<IActionResult> Index()
            => View(await _context.EVENT_APPROVAL.ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.EVENT_APPROVAL.FirstOrDefaultAsync(m => m.ApprovalID == id);
            if (item == null) return NotFound();
            return View(item);
        }

        public IActionResult Create() => View();

        // ===== START OF CHANGED CODE =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ApprovalID,EventID,ApprovedByUserID,Decision,DecisionNote,DecisionDateTime,member_id")] EVENT_APPROVAL item)
        {
            if (ModelState.IsValid)
            {
                // Ensure non-nullable fields have values
                item.DecisionNote = item.DecisionNote ?? "";

                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }
        // ===== END OF CHANGED CODE =====

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.EVENT_APPROVAL.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        // ===== START OF CHANGED CODE =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EVENT_APPROVAL item)
        {
            if (id != item.ApprovalID) return NotFound();

            if (ModelState.IsValid)
            {
                // Ensure non-nullable fields have values
                item.DecisionNote = item.DecisionNote ?? "";

                _context.Update(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }
        // ===== END OF CHANGED CODE =====

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.EVENT_APPROVAL.FirstOrDefaultAsync(m => m.ApprovalID == id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.EVENT_APPROVAL.FindAsync(id);
            if (item != null) _context.EVENT_APPROVAL.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}