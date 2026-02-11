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
    public class USERsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public USERsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: USERs
        public async Task<IActionResult> Index()
        {
            return View(await _context.USER.ToListAsync());
        }

        // GET: USERs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uSER = await _context.USER
                .FirstOrDefaultAsync(m => m.member_id == id);
            if (uSER == null)
            {
                return NotFound();
            }

            return View(uSER);
        }

        // GET: USERs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: USERs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("member_id,FullName,Email,phone,passwordHash,userType,UniversityNumber,isverified,status,createdAt,ApprovalID")] USER uSER)
        {
            if (ModelState.IsValid)
            {
                _context.Add(uSER);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(uSER);
        }

        // GET: USERs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uSER = await _context.USER.FindAsync(id);
            if (uSER == null)
            {
                return NotFound();
            }
            return View(uSER);
        }

        // POST: USERs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("member_id,FullName,Email,phone,passwordHash,userType,UniversityNumber,isverified,status,createdAt,ApprovalID")] USER uSER)
        {
            if (id != uSER.member_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(uSER);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!USERExists(uSER.member_id))
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
            return View(uSER);
        }

        // GET: USERs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uSER = await _context.USER
                .FirstOrDefaultAsync(m => m.member_id == id);
            if (uSER == null)
            {
                return NotFound();
            }

            return View(uSER);
        }

        // POST: USERs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var uSER = await _context.USER.FindAsync(id);
            if (uSER != null)
            {
                _context.USER.Remove(uSER);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool USERExists(int id)
        {
            return _context.USER.Any(e => e.member_id == id);
        }
    }
}
