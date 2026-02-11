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
    public class ATTENDANCEsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ATTENDANCEsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ATTENDANCEs
        public async Task<IActionResult> Index()
        {
            return View(await _context.ATTENDANCE.ToListAsync());
        }

        // GET: ATTENDANCEs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aTTENDANCE = await _context.ATTENDANCE
                .FirstOrDefaultAsync(m => m.AttendanceID == id);
            if (aTTENDANCE == null)
            {
                return NotFound();
            }

            return View(aTTENDANCE);
        }

        // GET: ATTENDANCEs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ATTENDANCEs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AttendanceID,EventID,UserID,TicketID,CheckedInAt,CheckInStatus")] ATTENDANCE aTTENDANCE)
        {
            if (ModelState.IsValid)
            {
                _context.Add(aTTENDANCE);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(aTTENDANCE);
        }

        // GET: ATTENDANCEs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aTTENDANCE = await _context.ATTENDANCE.FindAsync(id);
            if (aTTENDANCE == null)
            {
                return NotFound();
            }
            return View(aTTENDANCE);
        }

        // POST: ATTENDANCEs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AttendanceID,EventID,UserID,TicketID,CheckedInAt,CheckInStatus")] ATTENDANCE aTTENDANCE)
        {
            if (id != aTTENDANCE.AttendanceID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(aTTENDANCE);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ATTENDANCEExists(aTTENDANCE.AttendanceID))
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
            return View(aTTENDANCE);
        }

        // GET: ATTENDANCEs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aTTENDANCE = await _context.ATTENDANCE
                .FirstOrDefaultAsync(m => m.AttendanceID == id);
            if (aTTENDANCE == null)
            {
                return NotFound();
            }

            return View(aTTENDANCE);
        }

        // POST: ATTENDANCEs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var aTTENDANCE = await _context.ATTENDANCE.FindAsync(id);
            if (aTTENDANCE != null)
            {
                _context.ATTENDANCE.Remove(aTTENDANCE);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ATTENDANCEExists(int id)
        {
            return _context.ATTENDANCE.Any(e => e.AttendanceID == id);
        }
    }
}
