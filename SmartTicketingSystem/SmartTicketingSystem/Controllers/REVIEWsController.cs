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
    public class REVIEWsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public REVIEWsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: REVIEWs
        public async Task<IActionResult> Index()
        {
            return View(await _context.REVIEW.ToListAsync());
        }

        // GET: REVIEWs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rEVIEW = await _context.REVIEW
                .FirstOrDefaultAsync(m => m.ReviewID == id);
            if (rEVIEW == null)
            {
                return NotFound();
            }

            return View(rEVIEW);
        }

        // GET: REVIEWs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: REVIEWs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ReviewID,eventID,userID,Ratings,Comments,isVerifiedAttendee,ReviewStatus,createdAt")] REVIEW rEVIEW)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rEVIEW);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(rEVIEW);
        }

        // GET: REVIEWs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rEVIEW = await _context.REVIEW.FindAsync(id);
            if (rEVIEW == null)
            {
                return NotFound();
            }
            return View(rEVIEW);
        }

        // POST: REVIEWs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReviewID,eventID,userID,Ratings,Comments,isVerifiedAttendee,ReviewStatus,createdAt")] REVIEW rEVIEW)
        {
            if (id != rEVIEW.ReviewID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rEVIEW);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!REVIEWExists(rEVIEW.ReviewID))
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
            return View(rEVIEW);
        }

        // GET: REVIEWs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rEVIEW = await _context.REVIEW
                .FirstOrDefaultAsync(m => m.ReviewID == id);
            if (rEVIEW == null)
            {
                return NotFound();
            }

            return View(rEVIEW);
        }

        // POST: REVIEWs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rEVIEW = await _context.REVIEW.FindAsync(id);
            if (rEVIEW != null)
            {
                _context.REVIEW.Remove(rEVIEW);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool REVIEWExists(int id)
        {
            return _context.REVIEW.Any(e => e.ReviewID == id);
        }
    }
}
