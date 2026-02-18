using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartTicketingSystem.Data;
using SmartTicketingSystem.Models;

namespace SmartTicketingSystem.Controllers
{
    public class EVENTsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EVENTsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ----------------------------
        // Helpers: dropdown loaders
        // ----------------------------
        private async Task LoadDropDownsAsync(int? selectedCategoryId = null, int? selectedOrganizerUnitId = null)
        {
            var categories = await _context.EVENT_CATEGORY
                .OrderBy(c => c.categoryName)
                .ToListAsync();

            ViewData["categoryID"] = new SelectList(categories, "categoryID", "categoryName", selectedCategoryId);

            var organizerUnits = await _context.ORGANIZER_UNIT
                .OrderBy(o => o.UnitType)
                .ToListAsync();

            ViewData["organizerUnitID"] = new SelectList(organizerUnits, "OrganizerID", "UnitType", selectedOrganizerUnitId);
        }

        // SEARCH
        public async Task<IActionResult> Search(
            string mode,
            int? eventId,
            string title,
            string venue,
            string status,
            string visibility,
            char? isOnline,
            int? categoryId,
            int? organizerUnitId,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _context.Set<EVENT>().AsQueryable();

            if (mode == "EventID" && eventId.HasValue)
                query = query.Where(e => e.eventID == eventId.Value);

            else if (mode == "Title" && !string.IsNullOrWhiteSpace(title))
                query = query.Where(e => (e.title ?? "").Contains(title));

            else if (mode == "Venue" && !string.IsNullOrWhiteSpace(venue))
                query = query.Where(e => (e.venue ?? "").Contains(venue));

            else if (mode == "Status" && !string.IsNullOrWhiteSpace(status))
                query = query.Where(e => (e.status ?? "").Contains(status));

            else if (mode == "Visibility" && !string.IsNullOrWhiteSpace(visibility))
                query = query.Where(e => (e.visibility ?? "").Contains(visibility));

            else if (mode == "IsOnline" && isOnline.HasValue)
                query = query.Where(e => e.IsOnline == isOnline.Value);

            else if (mode == "DateRange")
            {
                if (fromDate.HasValue)
                    query = query.Where(e => e.StartDateTime >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(e => e.StartDateTime < toDate.Value.AddDays(1));
            }
            else if (mode == "Advanced")
            {
                if (eventId.HasValue) query = query.Where(e => e.eventID == eventId.Value);
                if (!string.IsNullOrWhiteSpace(title)) query = query.Where(e => (e.title ?? "").Contains(title));
                if (!string.IsNullOrWhiteSpace(venue)) query = query.Where(e => (e.venue ?? "").Contains(venue));
                if (!string.IsNullOrWhiteSpace(status)) query = query.Where(e => (e.status ?? "").Contains(status));
                if (!string.IsNullOrWhiteSpace(visibility)) query = query.Where(e => (e.visibility ?? "").Contains(visibility));
                if (isOnline.HasValue) query = query.Where(e => e.IsOnline == isOnline.Value);
                if (categoryId.HasValue) query = query.Where(e => e.categoryID == categoryId.Value);
                if (organizerUnitId.HasValue) query = query.Where(e => e.organizerUnitID == organizerUnitId.Value);
                if (fromDate.HasValue) query = query.Where(e => e.StartDateTime >= fromDate.Value);
                if (toDate.HasValue) query = query.Where(e => e.StartDateTime < toDate.Value.AddDays(1));
            }

            return View("Index", await query.ToListAsync());
        }

        // ----------------------------
        // GET: EVENTs
        // ----------------------------
        public async Task<IActionResult> Index()
        {
            return View(await _context.EVENT.ToListAsync());
        }

        // ----------------------------
        // GET: EVENTs/Details/5
        // ----------------------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var eVENT = await _context.EVENT.FirstOrDefaultAsync(m => m.eventID == id);
            if (eVENT == null) return NotFound();

            return View(eVENT);
        }

        
        // GET: EVENTs/Create
        // Only Admin or Organizer
        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Create()
        {
            await LoadDropDownsAsync();
            return View();
        }

        // POST: EVENTs/Create
        // Only Admin or Organizer
        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("eventID,title,Description,StartDateTime,endDateTime,venue,IsOnline,onlineLink,AccessibilityInfo,capacity,visibility,status,organizerInfo,Agenda,maplink,createdByUserID,organizerUnitID,categoryID,createdAt,updatedAt,ApprovalID")] EVENT eVENT)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropDownsAsync(eVENT.categoryID, eVENT.organizerUnitID);
                return View(eVENT);
            }

            // Optional defaults (safe):
            if (eVENT.createdAt == default) eVENT.createdAt = DateTime.Now;
            eVENT.updatedAt = DateTime.Now;

            _context.Add(eVENT);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: EVENTs/Edit/5
        // Only Admin or Organizer
        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var eVENT = await _context.EVENT.FindAsync(id);
            if (eVENT == null) return NotFound();

            await LoadDropDownsAsync(eVENT.categoryID, eVENT.organizerUnitID);
            return View(eVENT);
        }

        // POST: EVENTs/Edit/5
        // Only Admin or Organizer
        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("eventID,title,Description,StartDateTime,endDateTime,venue,IsOnline,onlineLink,AccessibilityInfo,capacity,visibility,status,organizerInfo,Agenda,maplink,createdByUserID,organizerUnitID,categoryID,createdAt,updatedAt,ApprovalID")] EVENT eVENT)
        {
            if (id != eVENT.eventID) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadDropDownsAsync(eVENT.categoryID, eVENT.organizerUnitID);
                return View(eVENT);
            }

            try
            {
                eVENT.updatedAt = DateTime.Now;
                _context.Update(eVENT);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EVENTExists(eVENT.eventID)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: EVENTs/Delete/5
        // Only Admin or Organizer
        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var eVENT = await _context.EVENT.FirstOrDefaultAsync(m => m.eventID == id);
            if (eVENT == null) return NotFound();

            return View(eVENT);
        }

        // POST: EVENTs/Delete/5
        // Only Admin or Organizer
        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eVENT = await _context.EVENT.FindAsync(id);
            if (eVENT != null) _context.EVENT.Remove(eVENT);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EVENTExists(int id)
        {
            return _context.EVENT.Any(e => e.eventID == id);
        }
    }
}
