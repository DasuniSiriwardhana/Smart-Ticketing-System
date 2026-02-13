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
    public class BOOKING_ITEMController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BOOKING_ITEMController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Search Bar
        public async Task<IActionResult> Search(
    string mode,
    int? bookingItemId,
    int? bookingId,
    int? ticketTypeId,
    int? quantity)
        {
            var query = _context.Set<BOOKING_ITEM>().AsQueryable();

            if (mode == "BookingItemID" && bookingItemId.HasValue)
                query = query.Where(x => x.BookingItemID == bookingItemId.Value);

            else if (mode == "BookingID" && bookingId.HasValue)
                query = query.Where(x => x.BookingID == bookingId.Value);

            else if (mode == "TicketTypeID" && ticketTypeId.HasValue)
                query = query.Where(x => x.TicketTypeID == ticketTypeId.Value);

            else if (mode == "Quantity" && quantity.HasValue)
                query = query.Where(x => x.Quantity == quantity.Value);

            else if (mode == "Advanced")
            {
                if (bookingItemId.HasValue) query = query.Where(x => x.BookingItemID == bookingItemId.Value);
                if (bookingId.HasValue) query = query.Where(x => x.BookingID == bookingId.Value);
                if (ticketTypeId.HasValue) query = query.Where(x => x.TicketTypeID == ticketTypeId.Value);
                if (quantity.HasValue) query = query.Where(x => x.Quantity == quantity.Value);
            }

            return View("Index", await query.ToListAsync());
        }

        // GET: BOOKING_ITEM
        public async Task<IActionResult> Index()
        {
            return View(await _context.BOOKING_ITEM.ToListAsync());
        }

        // GET: BOOKING_ITEM/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bOOKING_ITEM = await _context.BOOKING_ITEM
                .FirstOrDefaultAsync(m => m.BookingItemID == id);
            if (bOOKING_ITEM == null)
            {
                return NotFound();
            }

            return View(bOOKING_ITEM);
        }

        // GET: BOOKING_ITEM/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BOOKING_ITEM/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingItemID,BookingID,TicketTypeID,Quantity,UnitPrice,LineTotal")] BOOKING_ITEM bOOKING_ITEM)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bOOKING_ITEM);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bOOKING_ITEM);
        }

        // GET: BOOKING_ITEM/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bOOKING_ITEM = await _context.BOOKING_ITEM.FindAsync(id);
            if (bOOKING_ITEM == null)
            {
                return NotFound();
            }
            return View(bOOKING_ITEM);
        }

        // POST: BOOKING_ITEM/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingItemID,BookingID,TicketTypeID,Quantity,UnitPrice,LineTotal")] BOOKING_ITEM bOOKING_ITEM)
        {
            if (id != bOOKING_ITEM.BookingItemID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bOOKING_ITEM);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BOOKING_ITEMExists(bOOKING_ITEM.BookingItemID))
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
            return View(bOOKING_ITEM);
        }

        // GET: BOOKING_ITEM/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bOOKING_ITEM = await _context.BOOKING_ITEM
                .FirstOrDefaultAsync(m => m.BookingItemID == id);
            if (bOOKING_ITEM == null)
            {
                return NotFound();
            }

            return View(bOOKING_ITEM);
        }

        // POST: BOOKING_ITEM/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bOOKING_ITEM = await _context.BOOKING_ITEM.FindAsync(id);
            if (bOOKING_ITEM != null)
            {
                _context.BOOKING_ITEM.Remove(bOOKING_ITEM);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BOOKING_ITEMExists(int id)
        {
            return _context.BOOKING_ITEM.Any(e => e.BookingItemID == id);
        }
    }
}
