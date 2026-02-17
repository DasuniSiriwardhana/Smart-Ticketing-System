using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTicketingSystem.Data;
using SmartTicketingSystem.Models;
using Microsoft.AspNetCore.Authorization;

//Handling Authorizations  
[Authorize(Policy = "AdminOnly")]
public class RoleController : Controller
{
    public IActionResult Index() => View();
}

namespace SmartTicketingSystem.Controllers
{
    public class ATTENDANCEsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ATTENDANCEsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // SEARCH
        public async Task<IActionResult> Search(
            string mode,
            int? attendanceId,
            int? eventId,
            int? member_id,
            int? ticketId,
            string status)
        {
            var query = _context.Set<ATTENDANCE>().AsQueryable();

            if (mode == "AttendanceID" && attendanceId.HasValue)
                query = query.Where(a => a.AttendanceID == attendanceId.Value);

            else if (mode == "EventID" && eventId.HasValue)
                query = query.Where(a => a.EventID == eventId.Value);

            else if (mode == "MemberID" && member_id.HasValue)
                query = query.Where(a => a.member_id == member_id.Value);

            else if (mode == "TicketID" && ticketId.HasValue)
                query = query.Where(a => a.TicketID == ticketId.Value);

            else if (mode == "Status" && !string.IsNullOrWhiteSpace(status))
                query = query.Where(a => (a.CheckInStatus ?? "").Contains(status));

            else if (mode == "Advanced")
            {
                if (attendanceId.HasValue)
                    query = query.Where(a => a.AttendanceID == attendanceId.Value);

                if (eventId.HasValue)
                    query = query.Where(a => a.EventID == eventId.Value);

                if (member_id.HasValue)
                    query = query.Where(a => a.member_id == member_id.Value);

                if (ticketId.HasValue)
                    query = query.Where(a => a.TicketID == ticketId.Value);

                if (!string.IsNullOrWhiteSpace(status))
                    query = query.Where(a => (a.CheckInStatus ?? "").Contains(status));
            }

            return View(await query.ToListAsync());
        }

        // GET: ATTENDANCEs
        public async Task<IActionResult> Index()
        {
            return View(await _context.ATTENDANCE.ToListAsync());
        }

        // GET: ATTENDANCEs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var attendance = await _context.ATTENDANCE
                .FirstOrDefaultAsync(m => m.AttendanceID == id);

            if (attendance == null)
                return NotFound();

            return View(attendance);
        }

        // GET: ATTENDANCEs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ATTENDANCEs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("AttendanceID,EventID,member_id,TicketID,CheckedInAt,CheckInStatus")] ATTENDANCE attendance)
        {
            if (ModelState.IsValid)
            {
                _context.Add(attendance);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(attendance);
        }

        // GET: ATTENDANCEs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var attendance = await _context.ATTENDANCE.FindAsync(id);

            if (attendance == null)
                return NotFound();

            return View(attendance);
        }

        // POST: ATTENDANCEs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("AttendanceID,EventID,member_id,TicketID,CheckedInAt,CheckInStatus")] ATTENDANCE attendance)
        {
            if (id != attendance.AttendanceID)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(attendance);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ATTENDANCEExists(attendance.AttendanceID))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(attendance);
        }

        // GET: ATTENDANCEs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var attendance = await _context.ATTENDANCE
                .FirstOrDefaultAsync(m => m.AttendanceID == id);

            if (attendance == null)
                return NotFound();

            return View(attendance);
        }

        // POST: ATTENDANCEs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var attendance = await _context.ATTENDANCE.FindAsync(id);

            if (attendance != null)
                _context.ATTENDANCE.Remove(attendance);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ATTENDANCEExists(int id)
        {
            return _context.ATTENDANCE.Any(e => e.AttendanceID == id);
        }
    }
}
