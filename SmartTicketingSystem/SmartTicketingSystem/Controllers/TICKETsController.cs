using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTicketingSystem.Data;
using SmartTicketingSystem.Models;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace SmartTicketingSystem.Controllers
{
    [Authorize(Policy = "MemberOnly")]
    public class TICKETsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TICKETsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helpers

        private async Task<int?> GetCurrentMemberIdAsync()
        {
            var identityUserId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(identityUserId)) return null;

            return await _context.USER
                .Where(u => u.IdentityUserId == identityUserId)
                .Select(u => (int?)u.member_id)
                .FirstOrDefaultAsync();
        }

        private async Task<bool> CurrentUserHasAnyRoleAsync(params string[] roleNames)
        {
            var memberId = await GetCurrentMemberIdAsync();
            if (memberId == null) return false;

            return await (from ur in _context.USER_ROLE
                          join r in _context.Role on ur.roleID equals r.RoleId
                          where ur.member_id == memberId.Value
                                && roleNames.Contains(r.rolename)
                          select ur.UserRoleID).AnyAsync();
        }

        // Apply ownership filtering via BOOKING.member_id
        private async Task<IQueryable<TICKET>> ApplyTicketOwnershipFilterAsync(IQueryable<TICKET> query)
        {
            var isAdminOrOrg = await CurrentUserHasAnyRoleAsync("Admin", "Organizer");
            if (isAdminOrOrg) return query;

            var myId = await GetCurrentMemberIdAsync();
            if (myId == null) return query.Where(t => false);

            return from t in query
                   join b in _context.BOOKING on t.BookingID equals b.BookingID
                   where b.member_id == myId.Value
                   select t;
        }

        // SEARCH

        public async Task<IActionResult> Search(
            string mode,
            int? ticketId,
            int? bookingId,
            string qrCodeValue,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _context.TICKET.AsQueryable();
            query = await ApplyTicketOwnershipFilterAsync(query);

            if (mode == "TicketID" && ticketId.HasValue)
                query = query.Where(t => t.TicketID == ticketId.Value);

            else if (mode == "BookingID" && bookingId.HasValue)
                query = query.Where(t => t.BookingID == bookingId.Value);

            else if (mode == "QRCode" && !string.IsNullOrWhiteSpace(qrCodeValue))
                query = query.Where(t => (t.QRcodevalue ?? "").Contains(qrCodeValue));

            else if (mode == "DateRange")
            {
                if (fromDate.HasValue)
                    query = query.Where(t => t.issuedAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(t => t.issuedAt < toDate.Value.AddDays(1));
            }

            else if (mode == "Advanced")
            {
                if (ticketId.HasValue)
                    query = query.Where(t => t.TicketID == ticketId.Value);

                if (bookingId.HasValue)
                    query = query.Where(t => t.BookingID == bookingId.Value);

                if (!string.IsNullOrWhiteSpace(qrCodeValue))
                    query = query.Where(t => (t.QRcodevalue ?? "").Contains(qrCodeValue));

                if (fromDate.HasValue)
                    query = query.Where(t => t.issuedAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(t => t.issuedAt < toDate.Value.AddDays(1));
            }

            return View("Index",
                await query.OrderByDescending(t => t.issuedAt).ToListAsync());
        }

      
        // INDEX
     

        public async Task<IActionResult> Index()
        {
            var query = _context.TICKET.AsQueryable();
            query = await ApplyTicketOwnershipFilterAsync(query);

            return View(await query
                .OrderByDescending(t => t.issuedAt)
                .ToListAsync());
        }

    
        // DETAILS
       

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var query = _context.TICKET.AsQueryable();
            query = await ApplyTicketOwnershipFilterAsync(query);

            var ticket = await query
                .FirstOrDefaultAsync(t => t.TicketID == id);

            if (ticket == null) return NotFound();

            return View(ticket);
        }

        // QR CODE IMAGE ENDPOINT

        // Generates QR PNG image securely
        public async Task<IActionResult> Qr(int id)
        {
            var query = _context.TICKET.AsQueryable();
            query = await ApplyTicketOwnershipFilterAsync(query);

            var ticket = await query
                .FirstOrDefaultAsync(t => t.TicketID == id);

            if (ticket == null)
                return NotFound();

            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(ticket.QRcodevalue, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(data);
            using Bitmap bitmap = qrCode.GetGraphic(20);

            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);

            return File(ms.ToArray(), "image/png");
        }

        // ADMIN / ORGANIZER ONLY CRUD

        [Authorize(Policy = "AdminOrOrganizer")]
        public IActionResult Create() => View();

        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("TicketID,BookingID,QRcodevalue,issuedAt")] TICKET ticket)
        {
            if (ModelState.IsValid)
            {
                if (ticket.issuedAt == default)
                    ticket.issuedAt = DateTime.Now;

                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }

        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.TICKET.FindAsync(id);
            if (ticket == null) return NotFound();

            return View(ticket);
        }

        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("TicketID,BookingID,QRcodevalue,issuedAt")] TICKET ticket)
        {
            if (id != ticket.TicketID) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(ticket);
        }

        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.TICKET
                .FirstOrDefaultAsync(m => m.TicketID == id);

            if (ticket == null) return NotFound();

            return View(ticket);
        }

        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.TICKET.FindAsync(id);
            if (ticket != null)
                _context.TICKET.Remove(ticket);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
