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
    public class WAITING_LISTController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WAITING_LISTController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: WAITING_LIST
        public async Task<IActionResult> Index()
        {
            return View(await _context.WAITING_LIST.ToListAsync());
        }

        // GET: WAITING_LIST/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var wAITING_LIST = await _context.WAITING_LIST
                .FirstOrDefaultAsync(m => m.WaitingListID == id);
            if (wAITING_LIST == null)
            {
                return NotFound();
            }

            return View(wAITING_LIST);
        }

        // GET: WAITING_LIST/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: WAITING_LIST/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WaitingListID,EventID,UserID,AddedAt,Status")] WAITING_LIST wAITING_LIST)
        {
            if (ModelState.IsValid)
            {
                _context.Add(wAITING_LIST);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(wAITING_LIST);
        }

        // GET: WAITING_LIST/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var wAITING_LIST = await _context.WAITING_LIST.FindAsync(id);
            if (wAITING_LIST == null)
            {
                return NotFound();
            }
            return View(wAITING_LIST);
        }

        // POST: WAITING_LIST/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("WaitingListID,EventID,UserID,AddedAt,Status")] WAITING_LIST wAITING_LIST)
        {
            if (id != wAITING_LIST.WaitingListID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(wAITING_LIST);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WAITING_LISTExists(wAITING_LIST.WaitingListID))
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
            return View(wAITING_LIST);
        }

        // GET: WAITING_LIST/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var wAITING_LIST = await _context.WAITING_LIST
                .FirstOrDefaultAsync(m => m.WaitingListID == id);
            if (wAITING_LIST == null)
            {
                return NotFound();
            }

            return View(wAITING_LIST);
        }

        // POST: WAITING_LIST/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var wAITING_LIST = await _context.WAITING_LIST.FindAsync(id);
            if (wAITING_LIST != null)
            {
                _context.WAITING_LIST.Remove(wAITING_LIST);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WAITING_LISTExists(int id)
        {
            return _context.WAITING_LIST.Any(e => e.WaitingListID == id);
        }
    }
}
