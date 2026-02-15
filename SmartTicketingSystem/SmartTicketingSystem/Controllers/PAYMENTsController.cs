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
    public class PAYMENTsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PAYMENTsController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Search Bar
            public async Task<IActionResult> Search(
            string mode,
            int? paymentId,
            int? bookingId,
            string paymentMethod,
            string transactionReference,
            decimal? minAmount,
            decimal? maxAmount,
            DateTime? fromDate,
            DateTime? toDate)
    {
        var query = _context.Set<PAYMENT>().AsQueryable();

        if (mode == "PaymentID" && paymentId.HasValue)
            query = query.Where(p => p.PaymentID == paymentId.Value);

        else if (mode == "BookingID" && bookingId.HasValue)
            query = query.Where(p => p.BookingID == bookingId.Value);

        else if (mode == "PaymentMethod" && !string.IsNullOrWhiteSpace(paymentMethod))
            query = query.Where(p => (p.PaymentMethod ?? "").Contains(paymentMethod));

        else if (mode == "TransactionReference" && !string.IsNullOrWhiteSpace(transactionReference))
            query = query.Where(p => (p.TransactionReference ?? "").Contains(transactionReference));

        else if (mode == "AmountRange")
        {
            if (minAmount.HasValue)
                query = query.Where(p => p.Amount >= minAmount.Value);

            if (maxAmount.HasValue)
                query = query.Where(p => p.Amount <= maxAmount.Value);
        }

        else if (mode == "DateRange")
        {
            if (fromDate.HasValue)
                query = query.Where(p => p.PaidAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(p => p.PaidAt < toDate.Value.AddDays(1));
        }

        else if (mode == "Advanced")
        {
            if (paymentId.HasValue)
                query = query.Where(p => p.PaymentID == paymentId.Value);

            if (bookingId.HasValue)
                query = query.Where(p => p.BookingID == bookingId.Value);

            if (!string.IsNullOrWhiteSpace(paymentMethod))
                query = query.Where(p => (p.PaymentMethod ?? "").Contains(paymentMethod));

            if (!string.IsNullOrWhiteSpace(transactionReference))
                query = query.Where(p => (p.TransactionReference ?? "").Contains(transactionReference));

            if (minAmount.HasValue)
                query = query.Where(p => p.Amount >= minAmount.Value);

            if (maxAmount.HasValue)
                query = query.Where(p => p.Amount <= maxAmount.Value);

            if (fromDate.HasValue)
                query = query.Where(p => p.PaidAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(p => p.PaidAt < toDate.Value.AddDays(1));
        }

        return View("Index", await query.ToListAsync());
    }


    // GET: PAYMENTs
    public async Task<IActionResult> Index()
        {
            return View(await _context.PAYMENT.ToListAsync());
        }

        // GET: PAYMENTs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pAYMENT = await _context.PAYMENT
                .FirstOrDefaultAsync(m => m.PaymentID == id);
            if (pAYMENT == null)
            {
                return NotFound();
            }

            return View(pAYMENT);
        }

        // GET: PAYMENTs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PAYMENTs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PaymentID,BookingID,PaymentMethod,TransactionReference,Amount,PaidAt")] PAYMENT pAYMENT)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pAYMENT);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(pAYMENT);
        }

        // GET: PAYMENTs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pAYMENT = await _context.PAYMENT.FindAsync(id);
            if (pAYMENT == null)
            {
                return NotFound();
            }
            return View(pAYMENT);
        }

        // POST: PAYMENTs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PaymentID,BookingID,PaymentMethod,TransactionReference,Amount,PaidAt")] PAYMENT pAYMENT)
        {
            if (id != pAYMENT.PaymentID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pAYMENT);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PAYMENTExists(pAYMENT.PaymentID))
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
            return View(pAYMENT);
        }

        // GET: PAYMENTs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pAYMENT = await _context.PAYMENT
                .FirstOrDefaultAsync(m => m.PaymentID == id);
            if (pAYMENT == null)
            {
                return NotFound();
            }

            return View(pAYMENT);
        }

        // POST: PAYMENTs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pAYMENT = await _context.PAYMENT.FindAsync(id);
            if (pAYMENT != null)
            {
                _context.PAYMENT.Remove(pAYMENT);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PAYMENTExists(int id)
        {
            return _context.PAYMENT.Any(e => e.PaymentID == id);
        }
    }
}
