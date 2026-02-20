using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTicketingSystem.Data;
using SmartTicketingSystem.Models;
using SmartTicketingSystem.Models.ViewModels;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace SmartTicketingSystem.Controllers
{
    public class TICKETsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TICKETsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<int?> GetCurrentMemberIdAsync()
        {
            var identityUserId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(identityUserId)) return null;
            return await _context.USER
                .Where(u => u.IdentityUserId == identityUserId)
                .Select(u => (int?)u.member_id)
                .FirstOrDefaultAsync();
        }

        private bool IsAdmin() => User.IsInRole("Admin");

        // =========================
        // MY TICKETS - User's ticket dashboard
        // =========================
        [Authorize(Policy = "MemberOnly")]
        public async Task<IActionResult> MyTickets()
        {
            try
            {
                var memberId = await GetCurrentMemberIdAsync();
                if (memberId == null)
                {
                    TempData["Error"] = "Please login to view your tickets.";
                    return RedirectToAction("Login", "Account");
                }

                Console.WriteLine($"Loading tickets for member: {memberId}");

                var paidBookings = await _context.BOOKING
                    .Include(b => b.Event)
                    .Include(b => b.BookingItems)
                        .ThenInclude(bi => bi.TicketType)
                    .Include(b => b.Tickets)
                    .Where(b => b.member_id == memberId.Value && b.PaymentStatus == "Paid")
                    .OrderByDescending(b => b.Event.StartDateTime)
                    .ToListAsync();

                Console.WriteLine($"Found {paidBookings.Count} paid bookings");

                var viewModel = new MyTicketsVM();
                var now = DateTime.Now;

                foreach (var booking in paidBookings)
                {
                    var ticketGroup = new TicketGroupVM
                    {
                        BookingId = booking.BookingID,
                        BookingReference = booking.BookingReference,
                        EventTitle = booking.Event?.title ?? "Unknown Event",
                        EventDate = booking.Event?.StartDateTime ?? DateTime.Now,
                        EventVenue = booking.Event?.venue ?? "Unknown Venue",
                        Tickets = booking.Tickets?.Select(t => new TicketVM
                        {
                            TicketId = t.TicketID,
                            QRCodeValue = t.QRcodevalue,
                            IssuedAt = t.issuedAt
                        }).ToList() ?? new List<TicketVM>()
                    };

                    if (booking.Event?.StartDateTime > now)
                        viewModel.UpcomingTickets.Add(ticketGroup);
                    else
                        viewModel.PastTickets.Add(ticketGroup);
                }

                viewModel.Stats.TotalTickets = viewModel.UpcomingTickets.Sum(g => g.Tickets.Count) +
                                                viewModel.PastTickets.Sum(g => g.Tickets.Count);
                viewModel.Stats.UpcomingEvents = viewModel.UpcomingTickets.Count;
                viewModel.Stats.PastEvents = viewModel.PastTickets.Count;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MyTickets: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner: {ex.InnerException.Message}");

                TempData["Error"] = $"Error loading tickets: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        // =========================
        // QR CODE GENERATION
        // =========================
        [AllowAnonymous]
        public async Task<IActionResult> Qr(int id)
        {
            try
            {
                var ticket = await _context.TICKET
                    .Include(t => t.Booking)
                    .FirstOrDefaultAsync(t => t.TicketID == id);

                if (ticket == null) return NotFound();

                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(ticket.QRcodevalue, QRCodeGenerator.ECCLevel.Q))
                using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                {
                    byte[] qrCodeImage = qrCode.GetGraphic(20);
                    return File(qrCodeImage, "image/png");
                }
            }
            catch (Exception ex)
            {
                return Content($"Error generating QR code: {ex.Message}");
            }
        }

        // =========================
        // DOWNLOAD TICKET
        // =========================
        [Authorize(Policy = "MemberOnly")]
        public async Task<IActionResult> Download(int id)
        {
            try
            {
                var ticket = await _context.TICKET
                    .Include(t => t.Booking)
                    .FirstOrDefaultAsync(t => t.TicketID == id);

                if (ticket == null) return NotFound();

                var memberId = await GetCurrentMemberIdAsync();
                if (!IsAdmin() && ticket.Booking?.member_id != memberId)
                    return Forbid();

                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(ticket.QRcodevalue, QRCodeGenerator.ECCLevel.Q))
                using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                {
                    byte[] qrCodeImage = qrCode.GetGraphic(20);
                    return File(qrCodeImage, "image/png", $"ticket-{ticket.TicketID}.png");
                }
            }
            catch (Exception ex)
            {
                return Content($"Error downloading ticket: {ex.Message}");
            }
        }

        // =========================
        // SEARCH
        // =========================
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Search(
            string mode,
            int? ticketId,
            int? bookingId,
            string qrCodeValue,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _context.TICKET
                .Include(t => t.Booking)
                .ThenInclude(b => b.Event)
                .Include(t => t.Booking)
                .ThenInclude(b => b.User)
                .AsQueryable();

            if (mode == "TicketID" && ticketId.HasValue)
                query = query.Where(t => t.TicketID == ticketId.Value);

            else if (mode == "BookingID" && bookingId.HasValue)
                query = query.Where(t => t.BookingID == bookingId.Value);

            else if (mode == "QRCode" && !string.IsNullOrWhiteSpace(qrCodeValue))
                query = query.Where(t => t.QRcodevalue.Contains(qrCodeValue));

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
                    query = query.Where(t => t.QRcodevalue.Contains(qrCodeValue));
                if (fromDate.HasValue)
                    query = query.Where(t => t.issuedAt >= fromDate.Value);
                if (toDate.HasValue)
                    query = query.Where(t => t.issuedAt < toDate.Value.AddDays(1));
            }

            return View("Index", await query.OrderByDescending(t => t.issuedAt).ToListAsync());
        }

        // =========================
        // INDEX - Admin only
        // =========================
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.TICKET
                .Include(t => t.Booking)
                .ThenInclude(b => b.Event)
                .Include(t => t.Booking)
                .ThenInclude(b => b.User)
                .OrderByDescending(t => t.issuedAt)
                .ToListAsync());
        }

        // =========================
        // DETAILS
        // =========================
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var ticket = await _context.TICKET
                .Include(t => t.Booking)
                .ThenInclude(b => b.Event)
                .Include(t => t.Booking)
                .ThenInclude(b => b.User)
                .FirstOrDefaultAsync(m => m.TicketID == id);
            if (ticket == null) return NotFound();
            return View(ticket);
        }

        // =========================
        // CREATE (GET)
        // =========================
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Create()
        {
            return View();
        }

        // =========================
        // CREATE (POST)
        // =========================
        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingID,QRcodevalue,issuedAt")] TICKET ticket)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }

        // =========================
        // EDIT (GET)
        // =========================
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var ticket = await _context.TICKET.FindAsync(id);
            if (ticket == null) return NotFound();
            return View(ticket);
        }

        // =========================
        // EDIT (POST)
        // =========================
        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TicketID,BookingID,QRcodevalue,issuedAt")] TICKET ticket)
        {
            if (id != ticket.TicketID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.TICKET.Any(e => e.TicketID == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }

        // =========================
        // DELETE (GET)
        // =========================
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var ticket = await _context.TICKET
                .Include(t => t.Booking)
                .FirstOrDefaultAsync(m => m.TicketID == id);
            if (ticket == null) return NotFound();
            return View(ticket);
        }

        // =========================
        // DELETE (POST)
        // =========================
        [Authorize(Policy = "AdminOnly")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.TICKET.FindAsync(id);
            if (ticket != null)
            {
                _context.TICKET.Remove(ticket);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}