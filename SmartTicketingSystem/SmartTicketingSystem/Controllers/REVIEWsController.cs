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
    public class REVIEWsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public REVIEWsController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Search Bar

        public async Task<IActionResult> Search(
            string mode,
            int? reviewId,
            int? eventId,
            int? userId,
            int? minRating,
            int? maxRating,
            char? isVerifiedAttendee,
            string reviewStatus,
            string commentsText,
            DateTime? fromDate,
            DateTime? toDate)
    {
        var query = _context.Set<REVIEW>().AsQueryable();

        if (mode == "ReviewID" && reviewId.HasValue)
            query = query.Where(r => r.ReviewID == reviewId.Value);

        else if (mode == "EventID" && eventId.HasValue)
            query = query.Where(r => r.eventID == eventId.Value);

        else if (mode == "UserID" && userId.HasValue)
            query = query.Where(r => r.userID == userId.Value);

        else if (mode == "RatingRange")
        {
            if (minRating.HasValue)
                query = query.Where(r => r.Ratings >= minRating.Value);

            if (maxRating.HasValue)
                query = query.Where(r => r.Ratings <= maxRating.Value);
        }

        else if (mode == "Verified" && isVerifiedAttendee.HasValue)
            query = query.Where(r => r.isVerifiedAttendee == isVerifiedAttendee.Value);

        else if (mode == "ReviewStatus" && !string.IsNullOrWhiteSpace(reviewStatus))
            query = query.Where(r => r.ReviewStatus == reviewStatus);

        else if (mode == "Comments" && !string.IsNullOrWhiteSpace(commentsText))
            query = query.Where(r => (r.Comments ?? "").Contains(commentsText));

        else if (mode == "DateRange")
        {
            if (fromDate.HasValue)
                query = query.Where(r => r.createdAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(r => r.createdAt < toDate.Value.AddDays(1));
        }

        else if (mode == "Advanced")
        {
            if (reviewId.HasValue) query = query.Where(r => r.ReviewID == reviewId.Value);
            if (eventId.HasValue) query = query.Where(r => r.eventID == eventId.Value);
            if (userId.HasValue) query = query.Where(r => r.userID == userId.Value);

            if (minRating.HasValue) query = query.Where(r => r.Ratings >= minRating.Value);
            if (maxRating.HasValue) query = query.Where(r => r.Ratings <= maxRating.Value);

            if (isVerifiedAttendee.HasValue) query = query.Where(r => r.isVerifiedAttendee == isVerifiedAttendee.Value);

            if (!string.IsNullOrWhiteSpace(reviewStatus))
                query = query.Where(r => r.ReviewStatus == reviewStatus);

            if (!string.IsNullOrWhiteSpace(commentsText))
                query = query.Where(r => (r.Comments ?? "").Contains(commentsText));

            if (fromDate.HasValue) query = query.Where(r => r.createdAt >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(r => r.createdAt < toDate.Value.AddDays(1));
        }

        return View("Index", await query.ToListAsync());
    }

    // GET: REVIEWs
    public async Task<IActionResult> Index()
        {
            return View(await _context.REVIEW.ToListAsync());
        }

        // GET: REVIEWs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rEVIEW = await _context.REVIEW
                .FirstOrDefaultAsync(m => m.ReviewID == id);
            if (rEVIEW == null)
            {
                return NotFound();
            }

            return View(rEVIEW);
        }

        // GET: REVIEWs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: REVIEWs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ReviewID,eventID,userID,Ratings,Comments,isVerifiedAttendee,ReviewStatus,createdAt")] REVIEW rEVIEW)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rEVIEW);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(rEVIEW);
        }

        // GET: REVIEWs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rEVIEW = await _context.REVIEW.FindAsync(id);
            if (rEVIEW == null)
            {
                return NotFound();
            }
            return View(rEVIEW);
        }

        // POST: REVIEWs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReviewID,eventID,userID,Ratings,Comments,isVerifiedAttendee,ReviewStatus,createdAt")] REVIEW rEVIEW)
        {
            if (id != rEVIEW.ReviewID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rEVIEW);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!REVIEWExists(rEVIEW.ReviewID))
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
            return View(rEVIEW);
        }

        // GET: REVIEWs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rEVIEW = await _context.REVIEW
                .FirstOrDefaultAsync(m => m.ReviewID == id);
            if (rEVIEW == null)
            {
                return NotFound();
            }

            return View(rEVIEW);
        }

        // POST: REVIEWs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rEVIEW = await _context.REVIEW.FindAsync(id);
            if (rEVIEW != null)
            {
                _context.REVIEW.Remove(rEVIEW);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool REVIEWExists(int id)
        {
            return _context.REVIEW.Any(e => e.ReviewID == id);
        }
    }
}
