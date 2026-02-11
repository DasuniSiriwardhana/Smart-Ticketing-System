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
