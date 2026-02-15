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
    public class PUBLIC_EVENT_REQUESTController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PUBLIC_EVENT_REQUESTController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Search Bar
        
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
            if (createdFrom.HasValue)
                query = query.Where(r => r.CreatedAt >= createdFrom.Value);

            if (createdTo.HasValue)
                query = query.Where(r => r.CreatedAt < createdTo.Value.AddDays(1));
        }

        else if (mode == "ProposedDateRange")
        {
            if (proposedFrom.HasValue)
                query = query.Where(r => r.proposedDateTime >= proposedFrom.Value);

            if (proposedTo.HasValue)
                query = query.Where(r => r.proposedDateTime < proposedTo.Value.AddDays(1));
        }

        else if (mode == "Advanced")
        {
            if (requestId.HasValue) query = query.Where(r => r.requestID == requestId.Value);
            if (reviewedByUserId.HasValue) query = query.Where(r => r.ReviewedByUserID == reviewedByUserId.Value);

            if (!string.IsNullOrWhiteSpace(requestFullName))
                query = query.Where(r => (r.requestFullName ?? "").Contains(requestFullName));

            if (!string.IsNullOrWhiteSpace(requestEmail))
                query = query.Where(r => (r.RequestEmail ?? "").Contains(requestEmail));

            if (!string.IsNullOrWhiteSpace(eventTitle))
                query = query.Where(r => (r.eventTitle ?? "").Contains(eventTitle));

            if (!string.IsNullOrWhiteSpace(venueOrMode))
                query = query.Where(r => (r.VenueorMode ?? "").Contains(venueOrMode));

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(r => r.status == status);

            if (createdFrom.HasValue) query = query.Where(r => r.CreatedAt >= createdFrom.Value);
            if (createdTo.HasValue) query = query.Where(r => r.CreatedAt < createdTo.Value.AddDays(1));

            if (proposedFrom.HasValue) query = query.Where(r => r.proposedDateTime >= proposedFrom.Value);
            if (proposedTo.HasValue) query = query.Where(r => r.proposedDateTime < proposedTo.Value.AddDays(1));
        }

        return View("Index", await query.ToListAsync());
    }


    // GET: PUBLIC_EVENT_REQUEST
    public async Task<IActionResult> Index()
        {
            return View(await _context.PUBLIC_EVENT_REQUEST.ToListAsync());
        }

        // GET: PUBLIC_EVENT_REQUEST/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pUBLIC_EVENT_REQUEST = await _context.PUBLIC_EVENT_REQUEST
                .FirstOrDefaultAsync(m => m.requestID == id);
            if (pUBLIC_EVENT_REQUEST == null)
            {
                return NotFound();
            }

            return View(pUBLIC_EVENT_REQUEST);
        }

        // GET: PUBLIC_EVENT_REQUEST/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PUBLIC_EVENT_REQUEST/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("requestID,requestFullName,RequestEmail,phoneNumber,eventTitle,Description,proposedDateTime,VenueorMode,status,reviewedNote,ReviewedByUserID,CreatedAt")] PUBLIC_EVENT_REQUEST pUBLIC_EVENT_REQUEST)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pUBLIC_EVENT_REQUEST);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(pUBLIC_EVENT_REQUEST);
        }

        // GET: PUBLIC_EVENT_REQUEST/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pUBLIC_EVENT_REQUEST = await _context.PUBLIC_EVENT_REQUEST.FindAsync(id);
            if (pUBLIC_EVENT_REQUEST == null)
            {
                return NotFound();
            }
            return View(pUBLIC_EVENT_REQUEST);
        }

        // POST: PUBLIC_EVENT_REQUEST/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("requestID,requestFullName,RequestEmail,phoneNumber,eventTitle,Description,proposedDateTime,VenueorMode,status,reviewedNote,ReviewedByUserID,CreatedAt")] PUBLIC_EVENT_REQUEST pUBLIC_EVENT_REQUEST)
        {
            if (id != pUBLIC_EVENT_REQUEST.requestID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pUBLIC_EVENT_REQUEST);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PUBLIC_EVENT_REQUESTExists(pUBLIC_EVENT_REQUEST.requestID))
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
            return View(pUBLIC_EVENT_REQUEST);
        }

        // GET: PUBLIC_EVENT_REQUEST/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pUBLIC_EVENT_REQUEST = await _context.PUBLIC_EVENT_REQUEST
                .FirstOrDefaultAsync(m => m.requestID == id);
            if (pUBLIC_EVENT_REQUEST == null)
            {
                return NotFound();
            }

            return View(pUBLIC_EVENT_REQUEST);
        }

        // POST: PUBLIC_EVENT_REQUEST/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pUBLIC_EVENT_REQUEST = await _context.PUBLIC_EVENT_REQUEST.FindAsync(id);
            if (pUBLIC_EVENT_REQUEST != null)
            {
                _context.PUBLIC_EVENT_REQUEST.Remove(pUBLIC_EVENT_REQUEST);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PUBLIC_EVENT_REQUESTExists(int id)
        {
            return _context.PUBLIC_EVENT_REQUEST.Any(e => e.requestID == id);
        }
    }
}
