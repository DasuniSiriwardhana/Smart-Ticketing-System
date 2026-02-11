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
    public class EVENTsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EVENTsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: EVENTs
        public async Task<IActionResult> Index()
        {
            return View(await _context.EVENT.ToListAsync());
        }

        // GET: EVENTs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eVENT = await _context.EVENT
                .FirstOrDefaultAsync(m => m.eventID == id);
            if (eVENT == null)
            {
                return NotFound();
            }

            return View(eVENT);
        }

        // GET: EVENTs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EVENTs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("eventID,title,Description,StartDateTime,endDateTime,venue,IsOnline,onlineLink,AccessibilityInfo,capacity,visibility,status,organizerInfo,Agenda,maplink,createdByUserID,organizerUnitID,categoryID,createdAt,updatedAt,ApprovalID")] EVENT eVENT)
        {
            if (ModelState.IsValid)
            {
                _context.Add(eVENT);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(eVENT);
        }

        // GET: EVENTs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eVENT = await _context.EVENT.FindAsync(id);
            if (eVENT == null)
            {
                return NotFound();
            }
            return View(eVENT);
        }

        // POST: EVENTs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("eventID,title,Description,StartDateTime,endDateTime,venue,IsOnline,onlineLink,AccessibilityInfo,capacity,visibility,status,organizerInfo,Agenda,maplink,createdByUserID,organizerUnitID,categoryID,createdAt,updatedAt,ApprovalID")] EVENT eVENT)
        {
            if (id != eVENT.eventID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(eVENT);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EVENTExists(eVENT.eventID))
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
            return View(eVENT);
        }

        // GET: EVENTs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eVENT = await _context.EVENT
                .FirstOrDefaultAsync(m => m.eventID == id);
            if (eVENT == null)
            {
                return NotFound();
            }

            return View(eVENT);
        }

        // POST: EVENTs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eVENT = await _context.EVENT.FindAsync(id);
            if (eVENT != null)
            {
                _context.EVENT.Remove(eVENT);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EVENTExists(int id)
        {
            return _context.EVENT.Any(e => e.eventID == id);
        }
    }
}
