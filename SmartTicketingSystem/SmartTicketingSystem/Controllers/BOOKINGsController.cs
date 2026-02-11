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
    public class BOOKINGsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BOOKINGsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BOOKINGs
        public async Task<IActionResult> Index()
        {
            return View(await _context.BOOKING.ToListAsync());
        }

        // GET: BOOKINGs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bOOKING = await _context.BOOKING
                .FirstOrDefaultAsync(m => m.BookingID == id);
            if (bOOKING == null)
            {
                return NotFound();
            }

            return View(bOOKING);
        }

        // GET: BOOKINGs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BOOKINGs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingID,BookingReference,UserID,EventID,BookingDateTime,BookingStatus,TotalAmount,PaymentStatus,CancellationReason,CancelledAt,createdAt")] BOOKING bOOKING)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bOOKING);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bOOKING);
        }

        // GET: BOOKINGs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bOOKING = await _context.BOOKING.FindAsync(id);
            if (bOOKING == null)
            {
                return NotFound();
            }
            return View(bOOKING);
        }

        // POST: BOOKINGs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingID,BookingReference,UserID,EventID,BookingDateTime,BookingStatus,TotalAmount,PaymentStatus,CancellationReason,CancelledAt,createdAt")] BOOKING bOOKING)
        {
            if (id != bOOKING.BookingID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bOOKING);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BOOKINGExists(bOOKING.BookingID))
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
            return View(bOOKING);
        }

        // GET: BOOKINGs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bOOKING = await _context.BOOKING
                .FirstOrDefaultAsync(m => m.BookingID == id);
            if (bOOKING == null)
            {
                return NotFound();
            }

            return View(bOOKING);
        }

        // POST: BOOKINGs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bOOKING = await _context.BOOKING.FindAsync(id);
            if (bOOKING != null)
            {
                _context.BOOKING.Remove(bOOKING);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BOOKINGExists(int id)
        {
            return _context.BOOKING.Any(e => e.BookingID == id);
        }
    }
}
