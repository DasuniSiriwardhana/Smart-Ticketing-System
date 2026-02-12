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
    public class TICKET_TYPEController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TICKET_TYPEController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TICKET_TYPE
        public async Task<IActionResult> Index()
        {
            return View(await _context.TICKET_TYPE.ToListAsync());
        }

        // GET: TICKET_TYPE/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tICKET_TYPE = await _context.TICKET_TYPE
                .FirstOrDefaultAsync(m => m.TicketID == id);
            if (tICKET_TYPE == null)
            {
                return NotFound();
            }

            return View(tICKET_TYPE);
        }

        // GET: TICKET_TYPE/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TICKET_TYPE/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TicketID,EventID,TypeName,Price,seatLimit,salesStartAt,salesEndAt,isActive,createdAt")] TICKET_TYPE tICKET_TYPE)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tICKET_TYPE);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tICKET_TYPE);
        }

        // GET: TICKET_TYPE/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tICKET_TYPE = await _context.TICKET_TYPE.FindAsync(id);
            if (tICKET_TYPE == null)
            {
                return NotFound();
            }
            return View(tICKET_TYPE);
        }

        // POST: TICKET_TYPE/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TicketID,EventID,TypeName,Price,seatLimit,salesStartAt,salesEndAt,isActive,createdAt")] TICKET_TYPE tICKET_TYPE)
        {
            if (id != tICKET_TYPE.TicketID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tICKET_TYPE);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TICKET_TYPEExists(tICKET_TYPE.TicketID))
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
            return View(tICKET_TYPE);
        }

        // GET: TICKET_TYPE/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tICKET_TYPE = await _context.TICKET_TYPE
                .FirstOrDefaultAsync(m => m.TicketID == id);
            if (tICKET_TYPE == null)
            {
                return NotFound();
            }

            return View(tICKET_TYPE);
        }

        // POST: TICKET_TYPE/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tICKET_TYPE = await _context.TICKET_TYPE.FindAsync(id);
            if (tICKET_TYPE != null)
            {
                _context.TICKET_TYPE.Remove(tICKET_TYPE);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TICKET_TYPEExists(int id)
        {
            return _context.TICKET_TYPE.Any(e => e.TicketID == id);
        }
    }
}
