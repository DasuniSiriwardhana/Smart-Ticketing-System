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

        // =========================
        // INDEX - Admin only
        // =========================
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.PAYMENT.ToListAsync());
        }

        // =========================
        // CREATE (GET)
        // =========================
        public async Task<IActionResult> Create(int? bookingId = null)
        {
            await PopulateDropdownsAsync(bookingId);

            // Get booking details to display
            if (bookingId.HasValue)
            {
                var identityId = _userManager.GetUserId(User);
                var appUser = await _context.USER.FirstOrDefaultAsync(u => u.IdentityUserId == identityId);

                if (appUser != null)
                {
                    var booking = await _context.BOOKING
                        .FirstOrDefaultAsync(b => b.BookingID == bookingId && b.member_id == appUser.member_id);

                    ViewBag.Booking = booking;
                }
            }

            return View(new PAYMENT { BookingID = bookingId ?? 0 });
        }

        // =========================
        // CREATE (POST) 
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingID,PaymentMethod,TransactionReference")] PAYMENT payment)
        {
            try
            {
                var identityId = _userManager.GetUserId(User);
                var appUser = await _context.USER.FirstOrDefaultAsync(u => u.IdentityUserId == identityId);
                if (appUser == null)
                {
                    ModelState.AddModelError("", "Cannot find your profile.");
                    await PopulateDropdownsAsync(payment.BookingID);
                    return View(payment);
                }

                var booking = await _context.BOOKING
                    .Include(b => b.BookingItems)
                    .FirstOrDefaultAsync(b => b.BookingID == payment.BookingID && b.member_id == appUser.member_id);

                if (booking == null)
                {
                    ModelState.AddModelError("BookingID", "Invalid booking.");
                    await PopulateDropdownsAsync(payment.BookingID);
                    return View(payment);
                }

                if (booking.PaymentStatus == "Paid")
                {
                    ModelState.AddModelError("", "This booking is already paid.");
                    await PopulateDropdownsAsync(payment.BookingID);
                    return View(payment);
                }

                // Set payment details - DO NOT set Booking navigation property
                payment.Amount = booking.TotalAmount;
                payment.PaidAt = DateTime.Now;

                if (string.IsNullOrWhiteSpace(payment.TransactionReference))
                {
                    payment.TransactionReference = $"TXN-{Guid.NewGuid():N}".Substring(0, 14).ToUpper();
                }

                if (!ModelState.IsValid)
                {
                    await PopulateDropdownsAsync(payment.BookingID);
                    return View(payment);
                }

                using var tx = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Save payment - ONLY the scalar properties, not navigation
                    _context.PAYMENT.Add(payment);
                    await _context.SaveChangesAsync();

                    // Update booking
                    booking.PaymentStatus = "Paid";
                    booking.BookingStatus = "Confirmed";
                    _context.BOOKING.Update(booking);
                    await _context.SaveChangesAsync();

                    // Generate tickets
                    var bookingItems = await _context.BOOKING_ITEM
                        .Where(bi => bi.BookingID == booking.BookingID)
                        .ToListAsync();

                    foreach (var item in bookingItems)
                    {
                        for (int i = 0; i < item.Quantity; i++)
                        {
                            _context.TICKET.Add(new TICKET
                            {
                                BookingID = booking.BookingID,  // Only set the FK, not navigation
                                QRcodevalue = $"TICKET-{booking.BookingReference}-{Guid.NewGuid():N}",
                                issuedAt = DateTime.Now
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();

                    TempData["Success"] = "Payment successful! Your tickets have been generated.";
                    return RedirectToAction("Details", "BOOKINGs", new { id = booking.BookingID });
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();

                    var errorMessage = ex.Message;
                    if (ex.InnerException != null)
                        errorMessage += " | INNER: " + ex.InnerException.Message;

                    ModelState.AddModelError("", $"Payment failed: {errorMessage}");
                    await PopulateDropdownsAsync(payment.BookingID);
                    return View(payment);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                await PopulateDropdownsAsync(payment.BookingID);
                return View(payment);
            }
        }


        // =========================
        // Populate Dropdowns Helper
        // =========================
        private async Task PopulateDropdownsAsync(int? selectedBookingId = null)
        {
            var identityId = _userManager.GetUserId(User);
            var appUser = await _context.USER.FirstOrDefaultAsync(u => u.IdentityUserId == identityId);

            if (appUser != null)
            {
                var bookings = await _context.BOOKING
                    .Where(b => b.member_id == appUser.member_id && b.PaymentStatus != "Paid")
                    .OrderByDescending(b => b.BookingDateTime)
                    .Select(b => new
                    {
                        b.BookingID,
                        Display = $"Booking #{b.BookingID} | {b.BookingReference} | Rs. {b.TotalAmount:N2}"
                    })
                    .ToListAsync();

                ViewBag.BookingID = new SelectList(bookings, "BookingID", "Display", selectedBookingId);
            }
            else
            {
                ViewBag.BookingID = new SelectList(Enumerable.Empty<SelectListItem>());
            }

            ViewBag.PaymentMethod = new SelectList(new[] {
                "Card",
                "Cash",
                "BankTransfer",
                "MobileMoney"
            });
        }
    }
}