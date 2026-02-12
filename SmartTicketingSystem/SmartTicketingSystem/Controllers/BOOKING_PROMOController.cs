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
    public class BOOKING_PROMOController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BOOKING_PROMOController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BOOKING_PROMO
        public async Task<IActionResult> Index()
        {
            return View(await _context.BOOKING_PROMO.ToListAsync());
        }

        // GET: BOOKING_PROMO/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bOOKING_PROMO = await _context.BOOKING_PROMO
                .FirstOrDefaultAsync(m => m.BookingPromoID == id);
            if (bOOKING_PROMO == null)
            {
                return NotFound();
            }

            return View(bOOKING_PROMO);
        }

        // GET: BOOKING_PROMO/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BOOKING_PROMO/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingPromoID,BookingID,BookingCodeID,DiscountedAmount,AppliedAt")] BOOKING_PROMO bOOKING_PROMO)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bOOKING_PROMO);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bOOKING_PROMO);
        }

        // GET: BOOKING_PROMO/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bOOKING_PROMO = await _context.BOOKING_PROMO.FindAsync(id);
            if (bOOKING_PROMO == null)
            {
                return NotFound();
            }
            return View(bOOKING_PROMO);
        }

        // POST: BOOKING_PROMO/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingPromoID,BookingID,BookingCodeID,DiscountedAmount,AppliedAt")] BOOKING_PROMO bOOKING_PROMO)
        {
            if (id != bOOKING_PROMO.BookingPromoID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bOOKING_PROMO);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BOOKING_PROMOExists(bOOKING_PROMO.BookingPromoID))
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
            return View(bOOKING_PROMO);
        }

        // GET: BOOKING_PROMO/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bOOKING_PROMO = await _context.BOOKING_PROMO
                .FirstOrDefaultAsync(m => m.BookingPromoID == id);
            if (bOOKING_PROMO == null)
            {
                return NotFound();
            }

            return View(bOOKING_PROMO);
        }

        // POST: BOOKING_PROMO/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bOOKING_PROMO = await _context.BOOKING_PROMO.FindAsync(id);
            if (bOOKING_PROMO != null)
            {
                _context.BOOKING_PROMO.Remove(bOOKING_PROMO);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BOOKING_PROMOExists(int id)
        {
            return _context.BOOKING_PROMO.Any(e => e.BookingPromoID == id);
        }
    }
}
