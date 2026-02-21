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
        // MY TICKETS - FIXED WITH JOINS
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

                // Get all paid bookings for this user
                var paidBookings = await _context.BOOKING
                    .Where(b => b.member_id == memberId.Value && b.PaymentStatus == "Paid")
                    .OrderByDescending(b => b.BookingDateTime)
                    .ToListAsync();

                var viewModel = new MyTicketsVM();
                var now = DateTime.Now;

                foreach (var booking in paidBookings)
                {
                    // Get event details separately
                    var ev = await _context.EVENT.FindAsync(booking.EventID);

                    // Get tickets for this booking
                    var tickets = await _context.TICKET
                        .Where(t => t.BookingID == booking.BookingID)
                        .ToListAsync();

                    var ticketGroup = new TicketGroupVM
                    {
                        BookingId = booking.BookingID,
                        BookingReference = booking.BookingReference,
                        EventTitle = ev?.title ?? "Unknown Event",
                        EventDate = ev?.StartDateTime ?? DateTime.Now,
                        EventVenue = ev?.venue ?? "Unknown Venue",
                        Tickets = tickets.Select(t => new TicketVM
                        {
                            TicketId = t.TicketID,
                            QRCodeValue = t.QRcodevalue,
                            IssuedAt = t.issuedAt
                        }).ToList() ?? new List<TicketVM>()
                    };

                    if (ev?.StartDateTime > now)
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
                var ticket = await _context.TICKET.FindAsync(id);
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
                var ticket = await _context.TICKET.FindAsync(id);
                if (ticket == null) return NotFound();

                var memberId = await GetCurrentMemberIdAsync();
                var booking = await _context.BOOKING.FindAsync(ticket.BookingID);

                if (!IsAdmin() && booking?.member_id != memberId)
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
        // SEARCH - Admin only
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
            var query = _context.TICKET.AsQueryable();

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

            var tickets = await query.OrderByDescending(t => t.issuedAt).ToListAsync();

            // Get related data for display
            var viewModel = new List<TicketWithDetailsVM>();
            foreach (var ticket in tickets)
            {
                var booking = await _context.BOOKING.FindAsync(ticket.BookingID);
                var ev = booking != null ? await _context.EVENT.FindAsync(booking.EventID) : null;
                var user = booking != null ? await _context.USER.FindAsync(booking.member_id) : null;

                viewModel.Add(new TicketWithDetailsVM
                {
                    Ticket = ticket,
                    BookingReference = booking?.BookingReference,
                    EventTitle = ev?.title,
                    UserEmail = user?.Email
                });
            }

            return View("Index", viewModel);
        }

        // =========================
        // INDEX - Admin only (FIXED)
        // =========================
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Index()
        {
            var tickets = await _context.TICKET
                .OrderByDescending(t => t.issuedAt)
                .ToListAsync();

            var viewModel = new List<TicketWithDetailsVM>();
            foreach (var ticket in tickets)
            {
                var booking = await _context.BOOKING.FindAsync(ticket.BookingID);
                var ev = booking != null ? await _context.EVENT.FindAsync(booking.EventID) : null;
                var user = booking != null ? await _context.USER.FindAsync(booking.member_id) : null;

                viewModel.Add(new TicketWithDetailsVM
                {
                    Ticket = ticket,
                    BookingReference = booking?.BookingReference,
                    EventTitle = ev?.title,
                    UserEmail = user?.Email
                });
            }

            return View(viewModel);
        }

        // =========================
        // DETAILS - Admin only
        // =========================
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.TICKET.FindAsync(id);
            if (ticket == null) return NotFound();

            var booking = await _context.BOOKING.FindAsync(ticket.BookingID);
            var ev = booking != null ? await _context.EVENT.FindAsync(booking.EventID) : null;
            var user = booking != null ? await _context.USER.FindAsync(booking.member_id) : null;

            var viewModel = new TicketWithDetailsVM
            {
                Ticket = ticket,
                BookingReference = booking?.BookingReference,
                EventTitle = ev?.title,
                UserEmail = user?.Email
            };

            return View(viewModel);
        }

        // =========================
        // CREATE (GET) - Admin only
        // =========================
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Create()
        {
            return View();
        }

        // =========================
        // CREATE (POST) - Admin only
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
        // EDIT (GET) - Admin only
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
        // EDIT (POST) - Admin only
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
        // DELETE (GET) - Admin only
        // =========================
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var ticket = await _context.TICKET.FindAsync(id);
            if (ticket == null) return NotFound();
            return View(ticket);
        }

        // =========================
        // DELETE (POST) - Admin only
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

    // Helper ViewModel for tickets with details
    public class TicketWithDetailsVM
    {
        public TICKET Ticket { get; set; }
        public string? BookingReference { get; set; }
        public string? EventTitle { get; set; }
        public string? UserEmail { get; set; }
    }
}