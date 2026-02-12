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
    public class ORGANIZER_UNITController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ORGANIZER_UNITController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ORGANIZER_UNIT
        public async Task<IActionResult> Index()
        {
            return View(await _context.ORGANIZER_UNIT.ToListAsync());
        }

        // GET: ORGANIZER_UNIT/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var oRGANIZER_UNIT = await _context.ORGANIZER_UNIT
                .FirstOrDefaultAsync(m => m.OrganizerID == id);
            if (oRGANIZER_UNIT == null)
            {
                return NotFound();
            }

            return View(oRGANIZER_UNIT);
        }

        // GET: ORGANIZER_UNIT/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ORGANIZER_UNIT/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrganizerID,unitTime,UnitType,ContactEmail,ContactPhone,status,CreatedAt")] ORGANIZER_UNIT oRGANIZER_UNIT)
        {
            if (ModelState.IsValid)
            {
                _context.Add(oRGANIZER_UNIT);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(oRGANIZER_UNIT);
        }

        // GET: ORGANIZER_UNIT/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var oRGANIZER_UNIT = await _context.ORGANIZER_UNIT.FindAsync(id);
            if (oRGANIZER_UNIT == null)
            {
                return NotFound();
            }
            return View(oRGANIZER_UNIT);
        }

        // POST: ORGANIZER_UNIT/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrganizerID,unitTime,UnitType,ContactEmail,ContactPhone,status,CreatedAt")] ORGANIZER_UNIT oRGANIZER_UNIT)
        {
            if (id != oRGANIZER_UNIT.OrganizerID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(oRGANIZER_UNIT);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ORGANIZER_UNITExists(oRGANIZER_UNIT.OrganizerID))
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
            return View(oRGANIZER_UNIT);
        }

        // GET: ORGANIZER_UNIT/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var oRGANIZER_UNIT = await _context.ORGANIZER_UNIT
                .FirstOrDefaultAsync(m => m.OrganizerID == id);
            if (oRGANIZER_UNIT == null)
            {
                return NotFound();
            }

            return View(oRGANIZER_UNIT);
        }

        // POST: ORGANIZER_UNIT/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var oRGANIZER_UNIT = await _context.ORGANIZER_UNIT.FindAsync(id);
            if (oRGANIZER_UNIT != null)
            {
                _context.ORGANIZER_UNIT.Remove(oRGANIZER_UNIT);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ORGANIZER_UNITExists(int id)
        {
            return _context.ORGANIZER_UNIT.Any(e => e.OrganizerID == id);
        }
    }
}
