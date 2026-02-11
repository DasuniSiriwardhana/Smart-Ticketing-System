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
    public class USER_ROLEController : Controller
    {
        private readonly ApplicationDbContext _context;

        public USER_ROLEController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: USER_ROLE
        public async Task<IActionResult> Index()
        {
            return View(await _context.USER_ROLE.ToListAsync());
        }

        // GET: USER_ROLE/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uSER_ROLE = await _context.USER_ROLE
                .FirstOrDefaultAsync(m => m.UserRoleID == id);
            if (uSER_ROLE == null)
            {
                return NotFound();
            }

            return View(uSER_ROLE);
        }

        // GET: USER_ROLE/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: USER_ROLE/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserRoleID,roleID,userID,AssignedAt,member_id")] USER_ROLE uSER_ROLE)
        {
            if (ModelState.IsValid)
            {
                _context.Add(uSER_ROLE);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(uSER_ROLE);
        }

        // GET: USER_ROLE/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uSER_ROLE = await _context.USER_ROLE.FindAsync(id);
            if (uSER_ROLE == null)
            {
                return NotFound();
            }
            return View(uSER_ROLE);
        }

        // POST: USER_ROLE/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserRoleID,roleID,userID,AssignedAt,member_id")] USER_ROLE uSER_ROLE)
        {
            if (id != uSER_ROLE.UserRoleID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(uSER_ROLE);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!USER_ROLEExists(uSER_ROLE.UserRoleID))
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
            return View(uSER_ROLE);
        }

        // GET: USER_ROLE/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var uSER_ROLE = await _context.USER_ROLE
                .FirstOrDefaultAsync(m => m.UserRoleID == id);
            if (uSER_ROLE == null)
            {
                return NotFound();
            }

            return View(uSER_ROLE);
        }

        // POST: USER_ROLE/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var uSER_ROLE = await _context.USER_ROLE.FindAsync(id);
            if (uSER_ROLE != null)
            {
                _context.USER_ROLE.Remove(uSER_ROLE);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool USER_ROLEExists(int id)
        {
            return _context.USER_ROLE.Any(e => e.UserRoleID == id);
        }
    }
}
