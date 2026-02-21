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
    public class INQUIRiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public INQUIRiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // SEARCH (Admin only)
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Search(
            string mode,
            int? inquiryId,
            int? handleByUserId,
            string fullName,
            string email,
            string category,
            string status,
            string messageText,
            string responseNote,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _context.Set<INQUIRY>().AsQueryable();

            if (mode == "InquiryID" && inquiryId.HasValue)
                query = query.Where(i => i.InquiryID == inquiryId.Value);

            else if (mode == "HandleByUserID" && handleByUserId.HasValue)
                query = query.Where(i => i.HandleByUserID == handleByUserId.Value);

            else if (mode == "FullName" && !string.IsNullOrWhiteSpace(fullName))
                query = query.Where(i => (i.FullName ?? "").Contains(fullName));

            else if (mode == "Email" && !string.IsNullOrWhiteSpace(email))
                query = query.Where(i => (i.Email ?? "").Contains(email));

            else if (mode == "Category" && !string.IsNullOrWhiteSpace(category))
                query = query.Where(i => (i.category ?? "").Contains(category));

            else if (mode == "Status" && !string.IsNullOrWhiteSpace(status))
                query = query.Where(i => i.status == status);

            else if (mode == "Message" && !string.IsNullOrWhiteSpace(messageText))
                query = query.Where(i => (i.message ?? "").Contains(messageText));

            else if (mode == "ResponseNote" && !string.IsNullOrWhiteSpace(responseNote))
                query = query.Where(i => (i.ResponseNote ?? "").Contains(responseNote));

            else if (mode == "CreatedDateRange")
            {
                if (fromDate.HasValue)
                    query = query.Where(i => i.createdAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(i => i.createdAt < toDate.Value.AddDays(1));
            }

            return View("Index", await query.ToListAsync());
        }

        // VIEW ALL (Admin only)
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.INQUIRY.ToListAsync());
        }

        // DETAILS (Admin only)
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var inquiry = await _context.INQUIRY
                .FirstOrDefaultAsync(m => m.InquiryID == id);

            if (inquiry == null)
                return NotFound();

            return View(inquiry);
        }

        // CREATE (Public access allowed) - FIXED: Added eventId and eventTitle parameters
        [AllowAnonymous]
        public IActionResult Create(int? eventId, string eventTitle)
        {
            // Pass the event information to the view
            ViewBag.EventId = eventId;
            ViewBag.EventTitle = eventTitle;

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Email,category,message")] INQUIRY inquiry, int? eventId)
        {
            if (ModelState.IsValid)
            {
                inquiry.createdAt = DateTime.Now;
                inquiry.status = "Pending";

                // Optionally store the eventId if you want to link inquiries to events
                // You would need to add an EventId field to your INQUIRY model first
                // if (eventId.HasValue)
                //     inquiry.EventId = eventId.Value;

                _context.Add(inquiry);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Your question has been submitted successfully! We'll respond within 24 hours.";

                // Redirect back to the event details if coming from an event
                if (eventId.HasValue)
                {
                    return RedirectToAction("Details", "EVENTs", new { id = eventId });
                }

                return RedirectToAction("Index", "Home");
            }

            // If we got this far, something failed, redisplay form
            ViewBag.EventId = eventId;
            return View(inquiry);
        }

        // EDIT / RESPOND (Admin only)
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var inquiry = await _context.INQUIRY.FindAsync(id);
            if (inquiry == null)
                return NotFound();

            return View(inquiry);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, INQUIRY inquiry)
        {
            if (id != inquiry.InquiryID)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    inquiry.HandleAt = DateTime.Now;
                    inquiry.status = "Resolved";

                    _context.Update(inquiry);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.INQUIRY.Any(e => e.InquiryID == inquiry.InquiryID))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(inquiry);
        }

        // DELETE (Admin only)
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var inquiry = await _context.INQUIRY
                .FirstOrDefaultAsync(m => m.InquiryID == id);

            if (inquiry == null)
                return NotFound();

            return View(inquiry);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inquiry = await _context.INQUIRY.FindAsync(id);
            if (inquiry != null)
                _context.INQUIRY.Remove(inquiry);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}