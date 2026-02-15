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
    public class INQUIRiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public INQUIRiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Search Bar
public async Task<IActionResult> Search(
    string mode,
    int? inquiryId,
    int? handleByUserId,
    string fullName,
    string email,
    string category,
    string status,
    string messageText,
    string responseNote,
    DateTime? fromDate,
    DateTime? toDate)
    {
        var query = _context.Set<INQUIRY>().AsQueryable();

        if (mode == "InquiryID" && inquiryId.HasValue)
            query = query.Where(i => i.InquiryID == inquiryId.Value);

        else if (mode == "HandleByUserID" && handleByUserId.HasValue)
            query = query.Where(i => i.HandleByUserID == handleByUserId.Value);

        else if (mode == "FullName" && !string.IsNullOrWhiteSpace(fullName))
            query = query.Where(i => (i.FullName ?? "").Contains(fullName));

        else if (mode == "Email" && !string.IsNullOrWhiteSpace(email))
            query = query.Where(i => (i.Email ?? "").Contains(email));

        else if (mode == "Category" && !string.IsNullOrWhiteSpace(category))
            query = query.Where(i => (i.category ?? "").Contains(category));

        // (dropdown)
        else if (mode == "Status" && !string.IsNullOrWhiteSpace(status))
            query = query.Where(i => i.status == status);

        else if (mode == "Message" && !string.IsNullOrWhiteSpace(messageText))
            query = query.Where(i => (i.message ?? "").Contains(messageText));

        else if (mode == "ResponseNote" && !string.IsNullOrWhiteSpace(responseNote))
            query = query.Where(i => (i.ResponseNote ?? "").Contains(responseNote));

        else if (mode == "CreatedDateRange")
        {
            if (fromDate.HasValue)
                query = query.Where(i => i.createdAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(i => i.createdAt < toDate.Value.AddDays(1));
        }

        else if (mode == "Advanced")
        {
            if (inquiryId.HasValue)
                query = query.Where(i => i.InquiryID == inquiryId.Value);

            if (handleByUserId.HasValue)
                query = query.Where(i => i.HandleByUserID == handleByUserId.Value);

            if (!string.IsNullOrWhiteSpace(fullName))
                query = query.Where(i => (i.FullName ?? "").Contains(fullName));

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(i => (i.Email ?? "").Contains(email));

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(i => (i.category ?? "").Contains(category));

            // ✅ Status exact match (dropdown)
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(i => i.status == status);

            if (!string.IsNullOrWhiteSpace(messageText))
                query = query.Where(i => (i.message ?? "").Contains(messageText));

            if (!string.IsNullOrWhiteSpace(responseNote))
                query = query.Where(i => (i.ResponseNote ?? "").Contains(responseNote));

            if (fromDate.HasValue)
                query = query.Where(i => i.createdAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(i => i.createdAt < toDate.Value.AddDays(1));
        }

        return View("Index", await query.ToListAsync());
    }


    // GET: INQUIRies
    public async Task<IActionResult> Index()
        {
            return View(await _context.INQUIRY.ToListAsync());
        }

        // GET: INQUIRies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var iNQUIRY = await _context.INQUIRY
                .FirstOrDefaultAsync(m => m.InquiryID == id);
            if (iNQUIRY == null)
            {
                return NotFound();
            }

            return View(iNQUIRY);
        }

        // GET: INQUIRies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: INQUIRies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InquiryID,FullName,Email,category,message,status,createdAt,HandleByUserID,HandleAt,ResponseNote")] INQUIRY iNQUIRY)
        {
            if (ModelState.IsValid)
            {
                _context.Add(iNQUIRY);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(iNQUIRY);
        }

        // GET: INQUIRies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var iNQUIRY = await _context.INQUIRY.FindAsync(id);
            if (iNQUIRY == null)
            {
                return NotFound();
            }
            return View(iNQUIRY);
        }

        // POST: INQUIRies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("InquiryID,FullName,Email,category,message,status,createdAt,HandleByUserID,HandleAt,ResponseNote")] INQUIRY iNQUIRY)
        {
            if (id != iNQUIRY.InquiryID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(iNQUIRY);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!INQUIRYExists(iNQUIRY.InquiryID))
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
            return View(iNQUIRY);
        }

        // GET: INQUIRies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var iNQUIRY = await _context.INQUIRY
                .FirstOrDefaultAsync(m => m.InquiryID == id);
            if (iNQUIRY == null)
            {
                return NotFound();
            }

            return View(iNQUIRY);
        }

        // POST: INQUIRies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var iNQUIRY = await _context.INQUIRY.FindAsync(id);
            if (iNQUIRY != null)
            {
                _context.INQUIRY.Remove(iNQUIRY);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool INQUIRYExists(int id)
        {
            return _context.INQUIRY.Any(e => e.InquiryID == id);
        }
    }
}
