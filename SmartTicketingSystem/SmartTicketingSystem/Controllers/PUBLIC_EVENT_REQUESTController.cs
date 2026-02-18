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
    public class PUBLIC_EVENT_REQUESTController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PUBLIC_EVENT_REQUESTController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Admin/Organizer only: list + review requests
        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Search(
            string mode,
            int? requestId,
            int? reviewedByUserId,
            string requestFullName,
            string requestEmail,
            string eventTitle,
            string venueOrMode,
            string status,
            DateTime? createdFrom,
            DateTime? createdTo,
            DateTime? proposedFrom,
            DateTime? proposedTo)
        {
            var query = _context.Set<PUBLIC_EVENT_REQUEST>().AsQueryable();

            if (mode == "RequestID" && requestId.HasValue)
                query = query.Where(r => r.requestID == requestId.Value);

            else if (mode == "ReviewedByUserID" && reviewedByUserId.HasValue)
                query = query.Where(r => r.ReviewedByUserID == reviewedByUserId.Value);

            else if (mode == "FullName" && !string.IsNullOrWhiteSpace(requestFullName))
                query = query.Where(r => (r.requestFullName ?? "").Contains(requestFullName));

            else if (mode == "Email" && !string.IsNullOrWhiteSpace(requestEmail))
                query = query.Where(r => (r.RequestEmail ?? "").Contains(requestEmail));

            else if (mode == "EventTitle" && !string.IsNullOrWhiteSpace(eventTitle))
                query = query.Where(r => (r.eventTitle ?? "").Contains(eventTitle));

            else if (mode == "VenueOrMode" && !string.IsNullOrWhiteSpace(venueOrMode))
                query = query.Where(r => (r.VenueorMode ?? "").Contains(venueOrMode));

            else if (mode == "Status" && !string.IsNullOrWhiteSpace(status))
                query = query.Where(r => r.status == status);

            else if (mode == "CreatedDateRange")
            {
                if (createdFrom.HasValue) query = query.Where(r => r.CreatedAt >= createdFrom.Value);
                if (createdTo.HasValue) query = query.Where(r => r.CreatedAt < createdTo.Value.AddDays(1));
            }

            else if (mode == "ProposedDateRange")
            {
                if (proposedFrom.HasValue) query = query.Where(r => r.proposedDateTime >= proposedFrom.Value);
                if (proposedTo.HasValue) query = query.Where(r => r.proposedDateTime < proposedTo.Value.AddDays(1));
            }

            return View("Index", await query.ToListAsync());
        }

        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Index()
            => View(await _context.PUBLIC_EVENT_REQUEST.ToListAsync());

        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.PUBLIC_EVENT_REQUEST.FirstOrDefaultAsync(m => m.requestID == id);
            if (item == null) return NotFound();

            return View(item);
        }

        // Anyone can open Create page (Guest can request a public event)
        [AllowAnonymous]
        public IActionResult Create()
        {
            return View();
        }

        // Anyone can submit Create
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("requestID,requestFullName,RequestEmail,phoneNumber,eventTitle,Description,proposedDateTime,VenueorMode,status,reviewedNote,ReviewedByUserID,CreatedAt")] PUBLIC_EVENT_REQUEST item)
        {
            if (ModelState.IsValid)
            {
                item.CreatedAt = DateTime.Now;

                // default status if empty
                if (string.IsNullOrWhiteSpace(item.status))
                    item.status = "Pending";

                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Create)); // or redirect to a ThankYou page if you want
            }
            return View(item);
        }

        // ✅ Only Admin/Organizer can edit (review + approve/reject)
        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.PUBLIC_EVENT_REQUEST.FindAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }

        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("requestID,requestFullName,RequestEmail,phoneNumber,eventTitle,Description,proposedDateTime,VenueorMode,status,reviewedNote,ReviewedByUserID,CreatedAt")] PUBLIC_EVENT_REQUEST item)
        {
            if (id != item.requestID) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(item);
        }

        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.PUBLIC_EVENT_REQUEST.FirstOrDefaultAsync(m => m.requestID == id);
            if (item == null) return NotFound();

            return View(item);
        }

        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.PUBLIC_EVENT_REQUEST.FindAsync(id);
            if (item != null) _context.PUBLIC_EVENT_REQUEST.Remove(item);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PUBLIC_EVENT_REQUESTExists(int id)
        {
            return _context.PUBLIC_EVENT_REQUEST.Any(e => e.requestID == id);
        }
    }
}
