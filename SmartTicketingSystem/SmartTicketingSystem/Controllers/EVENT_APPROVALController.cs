using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartTicketingSystem.Data;
using SmartTicketingSystem.Models;

namespace SmartTicketingSystem.Controllers
{
    public class EVENT_APPROVALController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EVENT_APPROVALController(ApplicationDbContext context)
        {
            _context = context;
        }
        //Search Bar
        public async Task<IActionResult> Search(
    string mode,
    int? approvalId,
    int? eventId,
    int? approvedByUserId,
    int? memberId,
    char? decision,
    string decisionNote,
    DateTime? fromDate,
    DateTime? toDate)
        {
            var query = _context.Set<EVENT_APPROVAL>().AsQueryable();

            if (mode == "ApprovalID" && approvalId.HasValue)
                query = query.Where(x => x.ApprovalID == approvalId.Value);

            else if (mode == "EventID" && eventId.HasValue)
                query = query.Where(x => x.EventID == eventId.Value);

            else if (mode == "ApprovedByUserID" && approvedByUserId.HasValue)
                query = query.Where(x => x.ApprovedByUserID == approvedByUserId.Value);

            else if (mode == "MemberID" && memberId.HasValue)
                query = query.Where(x => x.member_id == memberId.Value);

            else if (mode == "Decision" && decision.HasValue)
                query = query.Where(x => x.Decision == decision.Value);

            else if (mode == "DecisionNote" && !string.IsNullOrWhiteSpace(decisionNote))
                query = query.Where(x => (x.DecisionNote ?? "").Contains(decisionNote));

            else if (mode == "DateRange")
            {
                if (fromDate.HasValue)
                    query = query.Where(x => x.DecisionDateTime >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(x => x.DecisionDateTime < toDate.Value.AddDays(1));
            }

            else if (mode == "Advanced")
            {
                if (approvalId.HasValue) query = query.Where(x => x.ApprovalID == approvalId.Value);
                if (eventId.HasValue) query = query.Where(x => x.EventID == eventId.Value);
                if (approvedByUserId.HasValue) query = query.Where(x => x.ApprovedByUserID == approvedByUserId.Value);
                if (memberId.HasValue) query = query.Where(x => x.member_id == memberId.Value);

                if (decision.HasValue) query = query.Where(x => x.Decision == decision.Value);

                if (!string.IsNullOrWhiteSpace(decisionNote))
                    query = query.Where(x => (x.DecisionNote ?? "").Contains(decisionNote));

                if (fromDate.HasValue)
                    query = query.Where(x => x.DecisionDateTime >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(x => x.DecisionDateTime < toDate.Value.AddDays(1));
            }

            return View("Index", await query.ToListAsync());
        }

        // GET: EVENT_APPROVAL
        public async Task<IActionResult> Index()
        {
            return View(await _context.EVENT_APPROVAL.ToListAsync());
        }

        // GET: EVENT_APPROVAL/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eVENT_APPROVAL = await _context.EVENT_APPROVAL
                .FirstOrDefaultAsync(m => m.ApprovalID == id);
            if (eVENT_APPROVAL == null)
            {
                return NotFound();
            }

            return View(eVENT_APPROVAL);
        }

        // GET: EVENT_APPROVAL/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EVENT_APPROVAL/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ApprovalID,EventID,ApprovedByUserID,Decision,DecisionNote,DecisionDateTime,member_id")] EVENT_APPROVAL eVENT_APPROVAL)
        {
            if (ModelState.IsValid)
            {
                _context.Add(eVENT_APPROVAL);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(eVENT_APPROVAL);
        }

        // GET: EVENT_APPROVAL/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eVENT_APPROVAL = await _context.EVENT_APPROVAL.FindAsync(id);
            if (eVENT_APPROVAL == null)
            {
                return NotFound();
            }
            return View(eVENT_APPROVAL);
        }

        // POST: EVENT_APPROVAL/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ApprovalID,EventID,ApprovedByUserID,Decision,DecisionNote,DecisionDateTime,member_id")] EVENT_APPROVAL eVENT_APPROVAL)
        {
            if (id != eVENT_APPROVAL.ApprovalID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(eVENT_APPROVAL);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EVENT_APPROVALExists(eVENT_APPROVAL.ApprovalID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(eVENT_APPROVAL);
        }

        // GET: EVENT_APPROVAL/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eVENT_APPROVAL = await _context.EVENT_APPROVAL
                .FirstOrDefaultAsync(m => m.ApprovalID == id);
            if (eVENT_APPROVAL == null)
            {
                return NotFound();
            }

            return View(eVENT_APPROVAL);
        }

        // POST: EVENT_APPROVAL/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eVENT_APPROVAL = await _context.EVENT_APPROVAL.FindAsync(id);
            if (eVENT_APPROVAL != null)
            {
                _context.EVENT_APPROVAL.Remove(eVENT_APPROVAL);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EVENT_APPROVALExists(int id)
        {
            return _context.EVENT_APPROVAL.Any(e => e.ApprovalID == id);
        }
    }
}
