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
    public class TICKETsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TICKETsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TICKETs
        public async Task<IActionResult> Index()
        {
            return View(await _context.TICKET.ToListAsync());
        }

        // GET: TICKETs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tICKET = await _context.TICKET
                .FirstOrDefaultAsync(m => m.TicketID == id);
            if (tICKET == null)
            {
                return NotFound();
            }

            return View(tICKET);
        }

        // GET: TICKETs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TICKETs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TicketID,BookingID,QRcodevalue,issuedAt")] TICKET tICKET)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tICKET);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tICKET);
        }

        // GET: TICKETs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tICKET = await _context.TICKET.FindAsync(id);
            if (tICKET == null)
            {
                return NotFound();
            }
            return View(tICKET);
        }

        // POST: TICKETs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TicketID,BookingID,QRcodevalue,issuedAt")] TICKET tICKET)
        {
            if (id != tICKET.TicketID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tICKET);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TICKETExists(tICKET.TicketID))
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
            return View(tICKET);
        }

        // GET: TICKETs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tICKET = await _context.TICKET
                .FirstOrDefaultAsync(m => m.TicketID == id);
            if (tICKET == null)
            {
                return NotFound();
            }

            return View(tICKET);
        }

        // POST: TICKETs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tICKET = await _context.TICKET.FindAsync(id);
            if (tICKET != null)
            {
                _context.TICKET.Remove(tICKET);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TICKETExists(int id)
        {
            return _context.TICKET.Any(e => e.TicketID == id);
        }
    }
}
