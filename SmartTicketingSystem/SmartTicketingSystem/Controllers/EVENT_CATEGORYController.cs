using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        // VIEW categories (members can see)
        [Authorize(Policy = "MemberOnly")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.EVENT_CATEGORY.ToListAsync());
        }

        // VIEW category details (members can see)
        [Authorize(Policy = "MemberOnly")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.EVENT_CATEGORY
                .FirstOrDefaultAsync(m => m.categoryID == id);

            if (category == null) return NotFound();

            return View(category);
        }

        // SEARCH categories (members can see)
        [Authorize(Policy = "MemberOnly")]
        public async Task<IActionResult> Search(
            string mode,
            int? categoryId,
            string categoryName,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _context.Set<EVENT_CATEGORY>().AsQueryable();

            if (mode == "CategoryID" && categoryId.HasValue)
                query = query.Where(c => c.categoryID == categoryId.Value);

            else if (mode == "CategoryName" && !string.IsNullOrWhiteSpace(categoryName))
                query = query.Where(c => c.categoryName.Contains(categoryName));

            else if (mode == "DateRange")
            {
                if (fromDate.HasValue)
                    query = query.Where(c => c.createdAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(c => c.createdAt < toDate.Value.AddDays(1));
            }
            else if (mode == "Advanced")
            {
                if (categoryId.HasValue)
                    query = query.Where(c => c.categoryID == categoryId.Value);

                if (!string.IsNullOrWhiteSpace(categoryName))
                    query = query.Where(c => c.categoryName.Contains(categoryName));

                if (fromDate.HasValue)
                    query = query.Where(c => c.createdAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(c => c.createdAt < toDate.Value.AddDays(1));
            }

            return View("Index", await query.ToListAsync());
        }

        // ADMIN ONLY: Create category
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Create()
        {
            return View();
        }

        // ADMIN ONLY: Create category
        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("categoryID,categoryName,createdAt")] EVENT_CATEGORY category)
        {
            if (ModelState.IsValid)
            {
                // If your form doesn't enter date, auto-set it:
                if (category.createdAt == default)
                    category.createdAt = DateTime.Now;

                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        //ADMIN ONLY: Edit category
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.EVENT_CATEGORY.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        // ADMIN ONLY: Edit category
        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("categoryID,categoryName,createdAt")] EVENT_CATEGORY category)
        {
            if (id != category.categoryID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EVENT_CATEGORYExists(category.categoryID))
                        return NotFound();

                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // ADMIN ONLY: Delete category
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.EVENT_CATEGORY
                .FirstOrDefaultAsync(m => m.categoryID == id);

            if (category == null) return NotFound();

            return View(category);
        }

        //ADMIN ONLY: Delete category
        [Authorize(Policy = "AdminOnly")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.EVENT_CATEGORY.FindAsync(id);

            if (category != null)
                _context.EVENT_CATEGORY.Remove(category);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EVENT_CATEGORYExists(int id)
        {
            return _context.EVENT_CATEGORY.Any(e => e.categoryID == id);
        }
    }
}
