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
    public class EVENT_CATEGORYController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EVENT_CATEGORYController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: EVENT_CATEGORY
        public async Task<IActionResult> Index()
        {
            return View(await _context.EVENT_CATEGORY.ToListAsync());
        }

        // GET: EVENT_CATEGORY/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eVENT_CATEGORY = await _context.EVENT_CATEGORY
                .FirstOrDefaultAsync(m => m.categoryID == id);
            if (eVENT_CATEGORY == null)
            {
                return NotFound();
            }

            return View(eVENT_CATEGORY);
        }

        // GET: EVENT_CATEGORY/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EVENT_CATEGORY/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("categoryID,categoryName,createdAt")] EVENT_CATEGORY eVENT_CATEGORY)
        {
            if (ModelState.IsValid)
            {
                _context.Add(eVENT_CATEGORY);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(eVENT_CATEGORY);
        }

        // GET: EVENT_CATEGORY/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eVENT_CATEGORY = await _context.EVENT_CATEGORY.FindAsync(id);
            if (eVENT_CATEGORY == null)
            {
                return NotFound();
            }
            return View(eVENT_CATEGORY);
        }

        // POST: EVENT_CATEGORY/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("categoryID,categoryName,createdAt")] EVENT_CATEGORY eVENT_CATEGORY)
        {
            if (id != eVENT_CATEGORY.categoryID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(eVENT_CATEGORY);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EVENT_CATEGORYExists(eVENT_CATEGORY.categoryID))
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
            return View(eVENT_CATEGORY);
        }

        // GET: EVENT_CATEGORY/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eVENT_CATEGORY = await _context.EVENT_CATEGORY
                .FirstOrDefaultAsync(m => m.categoryID == id);
            if (eVENT_CATEGORY == null)
            {
                return NotFound();
            }

            return View(eVENT_CATEGORY);
        }

        // POST: EVENT_CATEGORY/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eVENT_CATEGORY = await _context.EVENT_CATEGORY.FindAsync(id);
            if (eVENT_CATEGORY != null)
            {
                _context.EVENT_CATEGORY.Remove(eVENT_CATEGORY);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EVENT_CATEGORYExists(int id)
        {
            return _context.EVENT_CATEGORY.Any(e => e.categoryID == id);
        }
    }
}
