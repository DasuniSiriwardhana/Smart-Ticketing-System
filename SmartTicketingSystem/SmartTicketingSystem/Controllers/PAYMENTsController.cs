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
