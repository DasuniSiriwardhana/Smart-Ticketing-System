using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

        // SEARCH
        public async Task<IActionResult> Search(
            string mode,
            int? bookingId,
            int? member_id,
            int? eventId,
            string bookingStatus,
            string paymentStatus,
            string bookingReference)
        {
            var query = _context.Set<BOOKING>().AsQueryable();

            if (mode == "BookingID" && bookingId.HasValue)
                query = query.Where(b => b.BookingID == bookingId.Value);

            else if (mode == "MemberID" && member_id.HasValue)
                query = query.Where(b => b.member_id == member_id.Value);

            else if (mode == "EventID" && eventId.HasValue)
                query = query.Where(b => b.EventID == eventId.Value);

            else if (mode == "BookingStatus" && !string.IsNullOrWhiteSpace(bookingStatus))
                query = query.Where(b => (b.BookingStatus ?? "").Contains(bookingStatus));

            else if (mode == "PaymentStatus" && !string.IsNullOrWhiteSpace(paymentStatus))
                query = query.Where(b => (b.PaymentStatus ?? "").Contains(paymentStatus));

            else if (mode == "Reference" && !string.IsNullOrWhiteSpace(bookingReference))
                query = query.Where(b => (b.BookingReference ?? "").Contains(bookingReference));

            else if (mode == "Advanced")
            {
                if (bookingId.HasValue)
                    query = query.Where(b => b.BookingID == bookingId.Value);

                if (member_id.HasValue)
                    query = query.Where(b => b.member_id == member_id.Value);

                if (eventId.HasValue)
                    query = query.Where(b => b.EventID == eventId.Value);

                if (!string.IsNullOrWhiteSpace(bookingStatus))
                    query = query.Where(b => (b.BookingStatus ?? "").Contains(bookingStatus));

                if (!string.IsNullOrWhiteSpace(paymentStatus))
                    query = query.Where(b => (b.PaymentStatus ?? "").Contains(paymentStatus));

                if (!string.IsNullOrWhiteSpace(bookingReference))
                    query = query.Where(b => (b.BookingReference ?? "").Contains(bookingReference));
            }

            return View("Index", await query.ToListAsync());
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
                return NotFound();

            var booking = await _context.BOOKING
                .FirstOrDefaultAsync(m => m.BookingID == id);

            if (booking == null)
                return NotFound();

            return View(booking);
        }

        // GET: BOOKINGs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BOOKINGs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("BookingID,BookingReference,member_id,EventID,BookingDateTime,BookingStatus,TotalAmount,PaymentStatus,CancellationReason,CancelledAt,createdAt")] BOOKING booking)
        {
            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(booking);
        }

        // GET: BOOKINGs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var booking = await _context.BOOKING.FindAsync(id);

            if (booking == null)
                return NotFound();

            return View(booking);
        }

        // POST: BOOKINGs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("BookingID,BookingReference,member_id,EventID,BookingDateTime,BookingStatus,TotalAmount,PaymentStatus,CancellationReason,CancelledAt,createdAt")] BOOKING booking)
        {
            if (id != booking.BookingID)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BOOKINGExists(booking.BookingID))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(booking);
        }

        // GET: BOOKINGs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var booking = await _context.BOOKING
                .FirstOrDefaultAsync(m => m.BookingID == id);

            if (booking == null)
                return NotFound();

            return View(booking);
        }

        // POST: BOOKINGs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.BOOKING.FindAsync(id);

            if (booking != null)
                _context.BOOKING.Remove(booking);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BOOKINGExists(int id)
        {
            return _context.BOOKING.Any(e => e.BookingID == id);
        }
    }
}
