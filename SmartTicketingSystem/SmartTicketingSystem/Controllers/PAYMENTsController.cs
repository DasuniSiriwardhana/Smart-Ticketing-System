using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartTicketingSystem.Data;
using SmartTicketingSystem.Models;

namespace SmartTicketingSystem.Controllers
{
    [Authorize(Policy = "MemberOnly")]
    public class PAYMENTsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PAYMENTsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Admin only
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.PAYMENT.ToListAsync());
        }

        // GET: Create Payment
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View();
        }

        // POST: Create Payment (secure + generates tickets)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingID,PaymentType,PaymentMethod,TransactionReference")] PAYMENT payment)
        {
            // 1) Find current app user
            var identityId = _userManager.GetUserId(User);
            var appUser = await _context.USER.FirstOrDefaultAsync(u => u.IdentityUserId == identityId);
            if (appUser == null)
            {
                ModelState.AddModelError("", "Cannot find your profile. Please contact admin.");
                await PopulateDropdownsAsync();
                return View(payment);
            }

            // 2) Ensure booking belongs to this member
            var booking = await _context.BOOKING
                .FirstOrDefaultAsync(b => b.BookingID == payment.BookingID && b.member_id == appUser.member_id);

            if (booking == null)
            {
                ModelState.AddModelError("BookingID", "Invalid booking (not found or not yours).");
                await PopulateDropdownsAsync();
                return View(payment);
            }

            // 3) Prevent double payment
            var alreadyPaid = await _context.PAYMENT.AnyAsync(p => p.BookingID == booking.BookingID);
            if (alreadyPaid || string.Equals(booking.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "This booking is already paid.");
                await PopulateDropdownsAsync();
                return View(payment);
            }

            // 4) Server-side payment values
            payment.Amount = booking.TotalAmount;
            payment.PaidAt = DateTime.Now;

            if (string.IsNullOrWhiteSpace(payment.TransactionReference))
            {
                payment.TransactionReference = $"TXN-{Guid.NewGuid():N}".Substring(0, 14).ToUpper();
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(payment);
            }

            //  Use a transaction so payment + tickets succeed together
            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                // 5) Save payment
                _context.PAYMENT.Add(payment);

                // 6) Update booking status
                booking.PaymentStatus = "Paid";
                booking.BookingStatus = string.IsNullOrWhiteSpace(booking.BookingStatus) ? "Confirmed" : booking.BookingStatus;

                _context.BOOKING.Update(booking);
                await _context.SaveChangesAsync();

                // 7) Create Ticket(s)
                // If you want ONE ticket per booking, set ticketCount = 1
                // If you want ONE ticket per seat, use BOOKING_ITEM quantity (recommended).
                var ticketCount = await _context.BOOKING_ITEM
                    .Where(bi => bi.BookingID == booking.BookingID)
                    .SumAsync(bi => (int?)bi.Quantity) ?? 1;

                for (int i = 0; i < ticketCount; i++)
                {
                    var qrValue = $"BOOK-{booking.BookingID}-MEM-{booking.member_id}-T-{Guid.NewGuid():N}";

                    _context.TICKET.Add(new TICKET
                    {
                        BookingID = booking.BookingID,
                        QRcodevalue = qrValue,
                        issuedAt = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                // 8) Redirect with success message
                TempData["Success"] = "Payment successful! Your ticket QR code(s) have been generated.";

                // Option A: Redirect to booking details
                return RedirectToAction("Details", "BOOKINGs", new { id = booking.BookingID });

                // Option B (if you prefer): Redirect to tickets list
                // return RedirectToAction("Index", "TICKETs");
            }
            catch
            {
                await tx.RollbackAsync();
                ModelState.AddModelError("", "Payment failed. Please try again.");
                await PopulateDropdownsAsync();
                return View(payment);
            }
        }

        private async Task PopulateDropdownsAsync()
        {
            var identityId = _userManager.GetUserId(User);
            var appUser = await _context.USER.FirstOrDefaultAsync(u => u.IdentityUserId == identityId);

            if (appUser != null)
            {
                var bookings = await _context.BOOKING
                    .Where(b => b.member_id == appUser.member_id && (b.PaymentStatus == null || b.PaymentStatus != "Paid"))
                    .OrderByDescending(b => b.BookingDateTime)
                    .Select(b => new
                    {
                        b.BookingID,
                        Display = "Booking #" + b.BookingID + " | " + (b.BookingReference ?? "") + " | Rs " + b.TotalAmount
                    })
                    .ToListAsync();

                ViewBag.BookingID = new SelectList(bookings, "BookingID", "Display");
            }
            else
            {
                ViewBag.BookingID = new SelectList(Enumerable.Empty<SelectListItem>());
            }

            ViewBag.PaymentType = new SelectList(new[] { "Online", "Offline" });
            ViewBag.PaymentMethod = new SelectList(new[] { "Card", "Cash", "BankTransfer", "MobileMoney" });
        }
    }
}
