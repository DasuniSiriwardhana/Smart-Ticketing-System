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
    [Authorize(Policy = "AdminOnly")]
    public class RolesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public RolesController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Search(string mode, int? roleId, string rolename, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Set<Role>().AsQueryable();

            if (mode == "RoleID" && roleId.HasValue) query = query.Where(r => r.RoleId == roleId.Value);
            else if (mode == "RoleName" && !string.IsNullOrWhiteSpace(rolename)) query = query.Where(r => (r.rolename ?? "").Contains(rolename));
            else if (mode == "DateRange")
            {
                if (fromDate.HasValue) query = query.Where(r => r.createdAt >= fromDate.Value);
                if (toDate.HasValue) query = query.Where(r => r.createdAt < toDate.Value.AddDays(1));
            }
            else if (mode == "Advanced")
            {
                if (roleId.HasValue) query = query.Where(r => r.RoleId == roleId.Value);
                if (!string.IsNullOrWhiteSpace(rolename)) query = query.Where(r => (r.rolename ?? "").Contains(rolename));
                if (fromDate.HasValue) query = query.Where(r => r.createdAt >= fromDate.Value);
                if (toDate.HasValue) query = query.Where(r => r.createdAt < toDate.Value.AddDays(1));
            }

            return View("Index", await query.ToListAsync());
        }

        public async Task<IActionResult> Index() => View(await _context.Role.ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var role = await _context.Role.FirstOrDefaultAsync(m => m.RoleId == id);
            if (role == null) return NotFound();
            return View(role);
        }

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoleId,rolename,createdAt")] Role role)
        {
            if (!ModelState.IsValid) return View(role);
            if (role.createdAt == default) role.createdAt = DateTime.Now;
            _context.Add(role);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var role = await _context.Role.FindAsync(id);
            if (role == null) return NotFound();
            return View(role);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RoleId,rolename,createdAt")] Role role)
        {
            if (id != role.RoleId) return NotFound();
            if (!ModelState.IsValid) return View(role);

            try { _context.Update(role); await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Role.Any(e => e.RoleId == role.RoleId)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var role = await _context.Role.FirstOrDefaultAsync(m => m.RoleId == id);
            if (role == null) return NotFound();
            return View(role);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var role = await _context.Role.FindAsync(id);
            if (role != null) _context.Role.Remove(role);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
