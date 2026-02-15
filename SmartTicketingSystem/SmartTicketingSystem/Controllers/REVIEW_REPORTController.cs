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
    public class REVIEW_REPORTController : Controller
    {
        private readonly ApplicationDbContext _context;

        public REVIEW_REPORTController(ApplicationDbContext context)
        {
            _context = context;
        }
        //Search BAr Option 
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
            if (fromDate.HasValue)
                query = query.Where(r => r.ReportedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(r => r.ReportedAt < toDate.Value.AddDays(1));
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

        return View("Index", await query.ToListAsync());
    }


    // GET: REVIEW_REPORT
    public async Task<IActionResult> Index()
        {
            return View(await _context.REVIEW_REPORT.ToListAsync());
        }

        // GET: REVIEW_REPORT/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rEVIEW_REPORT = await _context.REVIEW_REPORT
                .FirstOrDefaultAsync(m => m.RportID == id);
            if (rEVIEW_REPORT == null)
            {
                return NotFound();
            }

            return View(rEVIEW_REPORT);
        }

        // GET: REVIEW_REPORT/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: REVIEW_REPORT/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RportID,ReviewID,ReportedByUserID,ReportReason,ReportDetail,ReportedAt")] REVIEW_REPORT rEVIEW_REPORT)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rEVIEW_REPORT);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(rEVIEW_REPORT);
        }

        // GET: REVIEW_REPORT/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rEVIEW_REPORT = await _context.REVIEW_REPORT.FindAsync(id);
            if (rEVIEW_REPORT == null)
            {
                return NotFound();
            }
            return View(rEVIEW_REPORT);
        }

        // POST: REVIEW_REPORT/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RportID,ReviewID,ReportedByUserID,ReportReason,ReportDetail,ReportedAt")] REVIEW_REPORT rEVIEW_REPORT)
        {
            if (id != rEVIEW_REPORT.RportID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rEVIEW_REPORT);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!REVIEW_REPORTExists(rEVIEW_REPORT.RportID))
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
            return View(rEVIEW_REPORT);
        }

        // GET: REVIEW_REPORT/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rEVIEW_REPORT = await _context.REVIEW_REPORT
                .FirstOrDefaultAsync(m => m.RportID == id);
            if (rEVIEW_REPORT == null)
            {
                return NotFound();
            }

            return View(rEVIEW_REPORT);
        }

        // POST: REVIEW_REPORT/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rEVIEW_REPORT = await _context.REVIEW_REPORT.FindAsync(id);
            if (rEVIEW_REPORT != null)
            {
                _context.REVIEW_REPORT.Remove(rEVIEW_REPORT);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool REVIEW_REPORTExists(int id)
        {
            return _context.REVIEW_REPORT.Any(e => e.RportID == id);
        }
    }
}
