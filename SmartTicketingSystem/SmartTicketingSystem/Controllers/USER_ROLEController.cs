using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

        // Search Bar
        public async Task<IActionResult> Search(
            string mode,
            int? userRoleId,
            int? roleId,
            int? member_id,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _context.Set<USER_ROLE>().AsQueryable();

            if (mode == "UserRoleID" && userRoleId.HasValue)
                query = query.Where(x => x.UserRoleID == userRoleId.Value);

            else if (mode == "RoleID" && roleId.HasValue)
                query = query.Where(x => x.roleID == roleId.Value);

            else if (mode == "MemberID" && member_id.HasValue)
                query = query.Where(x => x.member_id == member_id.Value);

            else if (mode == "DateRange")
            {
                if (fromDate.HasValue)
                    query = query.Where(x => x.AssignedAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(x => x.AssignedAt < toDate.Value.AddDays(1));
            }

            else if (mode == "Advanced")
            {
                if (userRoleId.HasValue)
                    query = query.Where(x => x.UserRoleID == userRoleId.Value);

                if (roleId.HasValue)
                    query = query.Where(x => x.roleID == roleId.Value);

                if (member_id.HasValue)
                    query = query.Where(x => x.member_id == member_id.Value);

                if (fromDate.HasValue)
                    query = query.Where(x => x.AssignedAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(x => x.AssignedAt < toDate.Value.AddDays(1));
            }

            return View("Index", await query
                .OrderByDescending(x => x.AssignedAt)
                .ToListAsync());
        }

        // GET: USER_ROLE
        public async Task<IActionResult> Index()
        {
            return View(await _context.USER_ROLE.ToListAsync());
        }

        // GET: USER_ROLE/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userRole = await _context.USER_ROLE
                .FirstOrDefaultAsync(m => m.UserRoleID == id);

            if (userRole == null) return NotFound();

            return View(userRole);
        }

        // GET: USER_ROLE/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: USER_ROLE/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserRoleID,roleID,member_id,AssignedAt")] USER_ROLE userRole)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userRole);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(userRole);
        }

        // GET: USER_ROLE/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userRole = await _context.USER_ROLE.FindAsync(id);
            if (userRole == null) return NotFound();

            return View(userRole);
        }

        // POST: USER_ROLE/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserRoleID,roleID,member_id,AssignedAt")] USER_ROLE userRole)
        {
            if (id != userRole.UserRoleID)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userRole);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.USER_ROLE.Any(e => e.UserRoleID == userRole.UserRoleID))
                        return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(userRole);
        }

        // GET: USER_ROLE/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userRole = await _context.USER_ROLE
                .FirstOrDefaultAsync(m => m.UserRoleID == id);

            if (userRole == null) return NotFound();

            return View(userRole);
        }

        // POST: USER_ROLE/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userRole = await _context.USER_ROLE.FindAsync(id);
            if (userRole != null)
                _context.USER_ROLE.Remove(userRole);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
