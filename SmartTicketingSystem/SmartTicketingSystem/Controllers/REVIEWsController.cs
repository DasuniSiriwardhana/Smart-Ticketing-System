using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTicketingSystem.Data;
using SmartTicketingSystem.Models;
using SmartTicketingSystem.Models.ViewModels;

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

        private bool IsAdmin() => User.IsInRole("Admin");

        // PUBLIC VIEW - Show approved reviews for an event
        [AllowAnonymous]
        public async Task<IActionResult> EventReviews(int eventId)
        {
            var ev = await _context.EVENT.FindAsync(eventId);
            if (ev == null) return NotFound();

            var reviews = await (from r in _context.REVIEW
                                 join u in _context.USER on r.member_id equals u.member_id
                                 where r.eventID == eventId && r.ReviewStatus == "Approved"
                                 orderby r.createdAt descending
                                 select new ReviewWithUserVM
                                 {
                                     ReviewID = r.ReviewID,
                                     eventID = r.eventID,
                                     member_id = r.member_id,
                                     Ratings = r.Ratings,
                                     Comments = r.Comments ?? "",
                                     isVerifiedAttendee = r.isVerifiedAttendee,
                                     ReviewStatus = r.ReviewStatus,
                                     createdAt = r.createdAt,
                                     UserFullName = u.FullName ?? "Anonymous",
                                     UserEmail = u.Email ?? ""
                                 }).ToListAsync();

            ViewBag.Event = ev;
            ViewBag.AverageRating = reviews.Any() ? reviews.Average(r => r.Ratings) : 0;
            ViewBag.TotalReviews = reviews.Count;

            return View(reviews);
        }

        // =========================
        // CREATE FOR EVENT (GET)
        // =========================
        [Authorize(Policy = "MemberOnly")]
        public async Task<IActionResult> Create(int eventId)
        {
            var memberId = await GetCurrentMemberIdAsync();
            if (memberId == null) return Forbid();

            // Check if user has paid booking
            var hasPaidBooking = await _context.BOOKING
                .AnyAsync(b => b.member_id == memberId.Value &&
                               b.EventID == eventId &&
                               b.PaymentStatus == "Paid");

            if (!hasPaidBooking)
            {
                TempData["Error"] = "You can only review events you have attended.";
                return RedirectToAction("Details", "EVENTs", new { id = eventId });
            }

            // Check if event has ended
            var ev = await _context.EVENT.FindAsync(eventId);
            if (ev == null || ev.endDateTime > DateTime.Now)
            {
                TempData["Error"] = "You can only review events after they have ended.";
                return RedirectToAction("Details", "EVENTs", new { id = eventId });
            }

            // Check if already reviewed
            var existingReview = await _context.REVIEW
                .FirstOrDefaultAsync(r => r.member_id == memberId.Value && r.eventID == eventId);

            if (existingReview != null)
            {
                TempData["Error"] = "You have already reviewed this event.";
                return RedirectToAction("Details", "EVENTs", new { id = eventId });
            }

            var review = new REVIEW
            {
                eventID = eventId,
                Ratings = 5,
                ReviewStatus = "Pending",
                isVerifiedAttendee = 'Y'
            };

            ViewBag.Event = ev;
            return View(review);
        }

        // =========================
        // CREATE (POST)
        // =========================
        [Authorize(Policy = "MemberOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(REVIEW review)
        {
            var memberId = await GetCurrentMemberIdAsync();
            if (memberId == null) return Forbid();

            var hasPaidBooking = await _context.BOOKING
                .AnyAsync(b => b.member_id == memberId.Value &&
                               b.EventID == review.eventID &&
                               b.PaymentStatus == "Paid");

            if (!hasPaidBooking)
            {
                ModelState.AddModelError("", "You can only review events you have attended.");
                ViewBag.Event = await _context.EVENT.FindAsync(review.eventID);
                return View(review);
            }

            review.member_id = memberId.Value;
            review.createdAt = DateTime.Now;
            review.isVerifiedAttendee = 'Y';
            review.ReviewStatus = "Pending";

            if (ModelState.IsValid)
            {
                _context.Add(review);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Your review has been submitted and is pending approval.";
                return RedirectToAction("Details", "EVENTs", new { id = review.eventID });
            }

            ViewBag.Event = await _context.EVENT.FindAsync(review.eventID);
            return View(review);
        }

        // =========================
        // ADMIN INDEX - All reviews
        // =========================
        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Index()
        {
            var reviews = await (from r in _context.REVIEW
                                 join u in _context.USER on r.member_id equals u.member_id
                                 join e in _context.EVENT on r.eventID equals e.eventID
                                 orderby r.createdAt descending
                                 select new ReviewWithUserVM
                                 {
                                     ReviewID = r.ReviewID,
                                     eventID = r.eventID,
                                     member_id = r.member_id,
                                     Ratings = r.Ratings,
                                     Comments = r.Comments ?? "",
                                     isVerifiedAttendee = r.isVerifiedAttendee,
                                     ReviewStatus = r.ReviewStatus,
                                     createdAt = r.createdAt,
                                     UserFullName = u.FullName ?? "Unknown",
                                     UserEmail = u.Email ?? "",
                                     EventTitle = e.title ?? "Unknown Event"
                                 }).ToListAsync();

            return View("AdminIndex", reviews);
        }

        // =========================
        // DETAILS - Admin view
        // =========================
        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var review = await (from r in _context.REVIEW
                                join u in _context.USER on r.member_id equals u.member_id
                                join e in _context.EVENT on r.eventID equals e.eventID
                                where r.ReviewID == id
                                select new ReviewWithUserVM
                                {
                                    ReviewID = r.ReviewID,
                                    eventID = r.eventID,
                                    member_id = r.member_id,
                                    Ratings = r.Ratings,
                                    Comments = r.Comments ?? "",
                                    isVerifiedAttendee = r.isVerifiedAttendee,
                                    ReviewStatus = r.ReviewStatus,
                                    createdAt = r.createdAt,
                                    UserFullName = u.FullName ?? "Unknown",
                                    UserEmail = u.Email ?? "",
                                    EventTitle = e.title ?? "Unknown Event"
                                }).FirstOrDefaultAsync();

            if (review == null) return NotFound();

            return View(review);
        }

        // =========================
        // APPROVE REVIEW
        // =========================
        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var review = await _context.REVIEW.FindAsync(id);
            if (review == null) return NotFound();

            review.ReviewStatus = "Approved";
            await _context.SaveChangesAsync();

            TempData["Success"] = "Review approved and published.";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // REJECT REVIEW
        // =========================
        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var review = await _context.REVIEW.FindAsync(id);
            if (review == null) return NotFound();

            review.ReviewStatus = "Rejected";
            await _context.SaveChangesAsync();

            TempData["Success"] = "Review rejected.";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // DELETE - Admin only
        // =========================
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var review = await (from r in _context.REVIEW
                                join u in _context.USER on r.member_id equals u.member_id
                                join e in _context.EVENT on r.eventID equals e.eventID
                                where r.ReviewID == id
                                select new ReviewWithUserVM
                                {
                                    ReviewID = r.ReviewID,
                                    eventID = r.eventID,
                                    member_id = r.member_id,
                                    Ratings = r.Ratings,
                                    Comments = r.Comments ?? "",
                                    ReviewStatus = r.ReviewStatus,
                                    createdAt = r.createdAt,
                                    UserFullName = u.FullName ?? "Unknown",
                                    EventTitle = e.title ?? "Unknown Event"
                                }).FirstOrDefaultAsync();

            if (review == null) return NotFound();

            return View(review);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.REVIEW.FindAsync(id);
            if (review != null)
            {
                _context.REVIEW.Remove(review);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}