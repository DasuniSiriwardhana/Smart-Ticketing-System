using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartTicketingSystem.Data;
using SmartTicketingSystem.Models;
using SmartTicketingSystem.Models.ViewModels;

namespace SmartTicketingSystem.Controllers
{
    public class EVENTsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EVENTsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =========================
        // Helpers
        // =========================
        private async Task<USER?> GetCurrentAppUserAsync()
        {
            var identityId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(identityId)) return null;

            return await _context.USER.FirstOrDefaultAsync(u => u.IdentityUserId == identityId);
        }

        private async Task LoadDropDownsAsync(int? selectedCategoryId = null, int? selectedOrganizerUnitId = null)
        {
            var categories = await _context.EVENT_CATEGORY
                .OrderBy(c => c.categoryName)
                .ToListAsync();

            ViewBag.categoryID = new SelectList(categories, "categoryID", "categoryName", selectedCategoryId);

            var organizerUnits = await _context.ORGANIZER_UNIT
                .OrderBy(o => o.UnitType)
                .ToListAsync();

            ViewBag.organizerUnitID = new SelectList(organizerUnits, "OrganizerID", "UnitType", selectedOrganizerUnitId);
        }

        private bool IsAdmin() => User.IsInRole("Admin");
        private bool IsOrganizer() => User.IsInRole("Organizer");
        private bool IsUniversityMember() => User.IsInRole("UniversityMember");
        private bool IsExternalMember() => User.IsInRole("ExternalMember");

        // =========================
        // INDEX
        // =========================
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;

            var baseQuery = _context.EVENT
                .Where(e => e.StartDateTime >= now)
                .AsQueryable();

            if (!User.Identity.IsAuthenticated)
            {
                var guest = await baseQuery
                    .Where(e => e.status == "Published" && e.visibility == "Public")
                    .OrderBy(e => e.StartDateTime)
                    .ToListAsync();
                return View(guest);
            }

            if (IsAdmin())
            {
                var admin = await baseQuery
                    .OrderBy(e => e.StartDateTime)
                    .ToListAsync();
                return View(admin);
            }

            USER? appUser = null;
            if (IsOrganizer())
                appUser = await GetCurrentAppUserAsync();

            if (IsExternalMember() && !IsUniversityMember())
            {
                var q = baseQuery.Where(e =>
                    (e.status == "Published" && e.visibility == "Public")
                    || (IsOrganizer() && appUser != null && e.createdByUserID == appUser.member_id)
                );
                var list = await q.OrderBy(e => e.StartDateTime).ToListAsync();
                return View(list);
            }

            if (IsUniversityMember())
            {
                var q = baseQuery.Where(e =>
                    e.status == "Published"
                    || e.status == "PendingUpcoming"
                    || (IsOrganizer() && appUser != null && e.createdByUserID == appUser.member_id)
                );
                var list = await q.OrderBy(e => e.StartDateTime).ToListAsync();
                return View(list);
            }

            var fallback = await baseQuery
                .Where(e => e.status == "Published" && e.visibility == "Public")
                .OrderBy(e => e.StartDateTime)
                .ToListAsync();
            return View(fallback);
        }

        // =========================
        // DETAILS
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.EVENT.FirstOrDefaultAsync(e => e.eventID == id.Value);
            if (ev == null) return NotFound();

            if (!User.Identity.IsAuthenticated)
            {
                if (!(ev.status == "Published" && ev.visibility == "Public"))
                    return Forbid();
            }
            else
            {
                if (IsExternalMember() && !IsUniversityMember())
                {
                    if (!(ev.status == "Published" && ev.visibility == "Public"))
                    {
                        if (IsOrganizer())
                        {
                            var u = await GetCurrentAppUserAsync();
                            if (u == null || ev.createdByUserID != u.member_id) return Forbid();
                        }
                        else return Forbid();
                    }
                }

                if (IsUniversityMember())
                {
                    if (!(ev.status == "Published" || ev.status == "PendingUpcoming"))
                    {
                        if (IsOrganizer())
                        {
                            var u = await GetCurrentAppUserAsync();
                            if (u == null || ev.createdByUserID != u.member_id) return Forbid();
                        }
                        else return Forbid();
                    }
                }
            }

            var now = DateTime.Now;
            var ticketTypes = await _context.TICKET_TYPE
                .Where(t => t.EventID == ev.eventID
                            && t.isActive == 'Y'
                            && t.salesStartAt <= now
                            && t.salesEndAt >= now)
                .OrderBy(t => t.Price)
                .ToListAsync();

            var bookedByTicketType = await _context.BOOKING_ITEM
                .Include(bi => bi.TicketType)
                .Where(bi => bi.TicketType != null && bi.TicketType.EventID == ev.eventID)
                .GroupBy(bi => bi.TicketTypeID)
                .Select(g => new { TicketID = g.Key, Qty = g.Sum(bi => bi.Quantity) })
                .ToListAsync();

            var bookedMap = bookedByTicketType.ToDictionary(x => x.TicketID, x => x.Qty);

            var remainingByTT = new Dictionary<int, int>();
            foreach (var tt in ticketTypes)
            {
                var booked = bookedMap.ContainsKey(tt.TicketID) ? bookedMap[tt.TicketID] : 0;
                remainingByTT[tt.TicketID] = Math.Max(0, tt.seatLimit - booked);
            }

            var totalBookedSeats = bookedByTicketType.Sum(x => x.Qty);
            var remainingEventCap = Math.Max(0, ev.capacity - totalBookedSeats);

            // Get active promotions for this event
            var activePromos = await _context.PROMO_CODE
                .Where(p => p.isActive == 'Y'
                            && p.startDate <= now
                            && p.endDate >= now)
                .ToListAsync();

            // Filter promos that have been used for this event
            var usedPromoIds = await _context.BOOKING_PROMO
                .Where(bp => bp.Booking != null && bp.Booking.EventID == ev.eventID)
                .Select(bp => bp.BookingCodeID)
                .Distinct()
                .ToListAsync();

            activePromos = activePromos
                .Where(p => !usedPromoIds.Contains(p.PromoCodeID))
                .ToList();

            ViewBag.ActivePromos = activePromos;

            var vm = new EventDetailsVM
            {
                Event = ev,
                TicketTypes = ticketTypes,
                RemainingByTicketType = remainingByTT,
                RemainingEventCapacity = remainingEventCap
            };

            return View(vm);
        }

        // =========================
        // CREATE (GET)
        // =========================
        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Create()
        {
            await LoadDropDownsAsync();
            return View(new EVENT
            {
                StartDateTime = DateTime.Now.AddDays(1),
                endDateTime = DateTime.Now.AddDays(1).AddHours(2),
                visibility = "University",
                IsOnline = 'N'
            });
        }

        // =========================
        // CREATE (POST)
        // =========================
        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("title,Description,StartDateTime,endDateTime,venue,IsOnlineBool,onlineLink,AccessibilityInfo,capacity,visibility,organizerInfo,Agenda,maplink,organizerUnitID,categoryID")]
            EVENT ev)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropDownsAsync(ev.categoryID, ev.organizerUnitID);
                return View(ev);
            }

            var identityId = _userManager.GetUserId(User);
            var appUser = await _context.USER.FirstOrDefaultAsync(u => u.IdentityUserId == identityId);
            if (appUser == null)
            {
                ModelState.AddModelError("", "Cannot find your profile.");
                await LoadDropDownsAsync(ev.categoryID, ev.organizerUnitID);
                return View(ev);
            }

            ev.IsOnline = ev.IsOnlineBool ? 'Y' : 'N';
            if (ev.IsOnline == 'N')
                ev.onlineLink = null;

            if (!string.Equals(ev.visibility, "Public", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(ev.visibility, "University", StringComparison.OrdinalIgnoreCase))
            {
                ev.visibility = "University";
            }

            ev.createdByUserID = appUser.member_id;
            ev.createdAt = DateTime.Now;
            ev.updatedAt = DateTime.Now;
            ev.status = "PendingApproval";
            ev.ApprovalID = 0;

            ev.title = ev.title ?? "";
            ev.Description = ev.Description ?? "";
            ev.venue = ev.venue ?? "";
            ev.onlineLink = ev.onlineLink ?? "";
            ev.AccessibilityInfo = ev.AccessibilityInfo ?? "";
            ev.organizerInfo = ev.organizerInfo ?? "";
            ev.Agenda = ev.Agenda ?? "";
            ev.maplink = ev.maplink ?? "";

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.EVENT.Add(ev);
                await _context.SaveChangesAsync();

                if (ev.visibility.Equals("Public", StringComparison.OrdinalIgnoreCase))
                {
                    string linkToken = $"EVENTLINK_{ev.eventID}_{DateTime.Now.Ticks}";

                    var req = new PUBLIC_EVENT_REQUEST
                    {
                        requestFullName = appUser.FullName ?? "",
                        RequestEmail = appUser.Email ?? "",
                        phoneNumber = appUser.phone ?? "",
                        eventTitle = ev.title ?? "",
                        Description = ev.Description ?? "",
                        proposedDateTime = ev.StartDateTime,
                        VenueorMode = (ev.IsOnline == 'Y')
                            ? ("Online: " + (ev.onlineLink ?? ""))
                            : (ev.venue ?? ""),
                        status = "Pending",
                        ReviewedByUserID = 0,
                        CreatedAt = DateTime.Now,
                        reviewedNote = $"EVENT_ID:{ev.eventID}|TOKEN:{linkToken}|Submitted on {DateTime.Now}"
                    };

                    _context.PUBLIC_EVENT_REQUEST.Add(req);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                TempData["Success"] = "Event submitted for admin approval.";
                return RedirectToAction(nameof(Details), new { id = ev.eventID });
            }
            catch
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Error creating event.");
                await LoadDropDownsAsync(ev.categoryID, ev.organizerUnitID);
                return View(ev);
            }
        }

        // =========================
        // PENDING APPROVALS
        // =========================
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> PendingApprovals()
        {
            var list = await _context.EVENT
                .Where(e => e.status == "PendingApproval" || e.status == "PendingUpcoming")
                .OrderBy(e => e.StartDateTime)
                .ToListAsync();
            return View(list);
        }

        // =========================
        // APPROVE EVENT
        // =========================
        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveEvent(int id, string decisionNote)
        {
            var ev = await _context.EVENT.FirstOrDefaultAsync(e => e.eventID == id);
            if (ev == null) return NotFound();

            if (ev.status != "PendingApproval" && ev.status != "PendingUpcoming")
            {
                TempData["Error"] = "Only pending events can be approved.";
                return RedirectToAction(nameof(PendingApprovals));
            }

            var adminUser = await GetCurrentAppUserAsync();
            if (adminUser == null)
            {
                TempData["Error"] = "Cannot find admin profile.";
                return RedirectToAction(nameof(PendingApprovals));
            }

            var approval = new EVENT_APPROVAL
            {
                EventID = ev.eventID,
                ApprovedByUserID = adminUser.member_id,
                Decision = 'A',
                DecisionNote = decisionNote ?? "",
                DecisionDateTime = DateTime.Now,
                member_id = ev.createdByUserID
            };

            _context.EVENT_APPROVAL.Add(approval);
            await _context.SaveChangesAsync();

            ev.status = "Published";
            ev.updatedAt = DateTime.Now;
            ev.ApprovalID = approval.ApprovalID;

            _context.EVENT.Update(ev);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Event approved and published.";
            return RedirectToAction(nameof(PendingApprovals));
        }

        // =========================
        // REJECT EVENT
        // =========================
        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectEvent(int id, string decisionNote)
        {
            var ev = await _context.EVENT.FirstOrDefaultAsync(e => e.eventID == id);
            if (ev == null) return NotFound();

            if (ev.status != "PendingApproval" && ev.status != "PendingUpcoming")
            {
                TempData["Error"] = "Only pending events can be rejected.";
                return RedirectToAction(nameof(PendingApprovals));
            }

            var adminUser = await GetCurrentAppUserAsync();
            if (adminUser == null)
            {
                TempData["Error"] = "Cannot find admin profile.";
                return RedirectToAction(nameof(PendingApprovals));
            }

            var approval = new EVENT_APPROVAL
            {
                EventID = ev.eventID,
                ApprovedByUserID = adminUser.member_id,
                Decision = 'R',
                DecisionNote = decisionNote ?? "",
                DecisionDateTime = DateTime.Now,
                member_id = ev.createdByUserID
            };

            _context.EVENT_APPROVAL.Add(approval);
            await _context.SaveChangesAsync();

            ev.status = "Rejected";
            ev.updatedAt = DateTime.Now;
            ev.ApprovalID = approval.ApprovalID;

            _context.EVENT.Update(ev);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Event rejected.";
            return RedirectToAction(nameof(PendingApprovals));
        }

        // =========================
        // BOOK (GET) - Booking form
        // =========================
        [Authorize(Policy = "MemberOnly")]
        public async Task<IActionResult> Book(int id)
        {
            try
            {
                var ev = await _context.EVENT
                    .FirstOrDefaultAsync(e => e.eventID == id && e.status == "Published");

                if (ev == null)
                {
                    TempData["Error"] = "Event not found.";
                    return RedirectToAction(nameof(Index));
                }

                var appUser = await GetCurrentAppUserAsync();
                if (appUser == null)
                {
                    TempData["Error"] = "Please login to book tickets.";
                    return RedirectToAction("Login", "Account");
                }

                if (IsExternalMember() && !IsUniversityMember() && ev.visibility != "Public")
                {
                    TempData["Error"] = "External members can only book public events.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var now = DateTime.Now;
                var ticketTypes = await _context.TICKET_TYPE
                    .Where(t => t.EventID == ev.eventID
                                && t.isActive == 'Y'
                                && t.salesStartAt <= now
                                && t.salesEndAt >= now)
                    .ToListAsync();

                if (!ticketTypes.Any())
                {
                    TempData["Error"] = "No tickets available.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var bookedSeats = await _context.BOOKING_ITEM
                    .Include(bi => bi.TicketType)
                    .Where(bi => bi.TicketType != null && bi.TicketType.EventID == ev.eventID)
                    .GroupBy(bi => bi.TicketTypeID)
                    .Select(g => new { TicketTypeID = g.Key, Booked = g.Sum(bi => bi.Quantity) })
                    .ToDictionaryAsync(x => x.TicketTypeID, x => x.Booked);

                var viewModel = new List<TicketTypeVM>();
                foreach (var tt in ticketTypes)
                {
                    bookedSeats.TryGetValue(tt.TicketID, out int booked);
                    viewModel.Add(new TicketTypeVM
                    {
                        TicketTypeId = tt.TicketID,
                        TypeName = tt.TypeName,
                        Price = tt.Price,
                        AvailableQuantity = Math.Max(0, tt.seatLimit - booked),
                        SeatLimit = tt.seatLimit
                    });
                }

                ViewBag.Event = ev;
                ViewBag.EventId = id;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // =========================
        // BOOK (POST) - Process booking with complete error handling
        // =========================
        [Authorize(Policy = "MemberOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(int eventId, Dictionary<int, int> quantities, string? promoCode = null)
        {
            try
            {
                Console.WriteLine("========== BOOK POST HIT ==========");
                Console.WriteLine($"EventId: {eventId}");
                Console.WriteLine($"PromoCode: {promoCode}");
                if (quantities != null)
                {
                    foreach (var q in quantities)
                    {
                        Console.WriteLine($"Key: {q.Key}, Value: {q.Value}");
                    }
                }

                // Check if quantities is null or empty
                if (quantities == null || !quantities.Any(kv => kv.Value > 0))
                {
                    Console.WriteLine("No quantities selected");
                    TempData["Error"] = "Please select at least one ticket.";
                    return RedirectToAction(nameof(Book), new { id = eventId });
                }

                var selectedTickets = quantities
                    .Where(kv => kv.Value > 0)
                    .ToDictionary(kv => kv.Key, kv => kv.Value);

                Console.WriteLine($"Selected tickets count: {selectedTickets.Count}");

                // Get current user
                var appUser = await GetCurrentAppUserAsync();
                if (appUser == null)
                {
                    Console.WriteLine("User not found");
                    TempData["Error"] = "Please login to book tickets.";
                    return RedirectToAction("Login", "Account");
                }
                Console.WriteLine($"User ID: {appUser.member_id}");

                // Get event
                var ev = await _context.EVENT
                    .FirstOrDefaultAsync(e => e.eventID == eventId && e.status == "Published");

                if (ev == null)
                {
                    Console.WriteLine("Event not found");
                    TempData["Error"] = "Event not found.";
                    return RedirectToAction(nameof(Index));
                }
                Console.WriteLine($"Event found: {ev.title}");

                // Check visibility rules
                if (IsExternalMember() && !IsUniversityMember() && ev.visibility != "Public")
                {
                    Console.WriteLine("Visibility check failed");
                    TempData["Error"] = "External members can only book public events.";
                    return RedirectToAction(nameof(Details), new { id = eventId });
                }

                // Get ticket types
                var now = DateTime.Now;
                var ticketTypes = await _context.TICKET_TYPE
                    .Where(t => t.EventID == eventId
                                && selectedTickets.Keys.Contains(t.TicketID)
                                && t.isActive == 'Y'
                                && t.salesStartAt <= now
                                && t.salesEndAt >= now)
                    .ToListAsync();

                if (ticketTypes.Count != selectedTickets.Count)
                {
                    Console.WriteLine($"Ticket types mismatch. Found: {ticketTypes.Count}, Expected: {selectedTickets.Count}");
                    TempData["Error"] = "One or more selected tickets are not available.";
                    return RedirectToAction(nameof(Book), new { id = eventId });
                }

                // Check availability
                var bookedSeats = await _context.BOOKING_ITEM
                    .Include(bi => bi.TicketType)
                    .Where(bi => bi.TicketType != null && bi.TicketType.EventID == eventId)
                    .GroupBy(bi => bi.TicketTypeID)
                    .Select(g => new { TicketTypeID = g.Key, Booked = g.Sum(bi => bi.Quantity) })
                    .ToDictionaryAsync(x => x.TicketTypeID, x => x.Booked);

                foreach (var tt in ticketTypes)
                {
                    bookedSeats.TryGetValue(tt.TicketID, out int booked);
                    var available = tt.seatLimit - booked;
                    Console.WriteLine($"Ticket {tt.TypeName}: Available={available}, Requested={selectedTickets[tt.TicketID]}");

                    if (selectedTickets[tt.TicketID] > available)
                    {
                        TempData["Error"] = $"Only {available} {tt.TypeName} tickets left.";
                        return RedirectToAction(nameof(Book), new { id = eventId });
                    }
                }

                // Check total capacity
                var totalBooked = await _context.BOOKING_ITEM
                    .Where(bi => bi.TicketType != null && bi.TicketType.EventID == eventId)
                    .SumAsync(bi => (int?)bi.Quantity) ?? 0;

                var totalRequested = selectedTickets.Values.Sum();
                Console.WriteLine($"Total booked: {totalBooked}, Total requested: {totalRequested}, Capacity: {ev.capacity}");

                if (totalBooked + totalRequested > ev.capacity)
                {
                    TempData["Info"] = "Event is full. Join waiting list?";
                    return RedirectToAction("Join", "WAITING_LIST", new { eventId });
                }

                // Calculate total
                decimal totalAmount = selectedTickets.Sum(kv =>
                    ticketTypes.First(tt => tt.TicketID == kv.Key).Price * kv.Value);
                Console.WriteLine($"Total amount: {totalAmount}");

                // Apply promo code if provided
                decimal discountAmount = 0;
                int? promoCodeId = null;

                if (!string.IsNullOrWhiteSpace(promoCode))
                {
                    var promo = await _context.PROMO_CODE
                        .FirstOrDefaultAsync(p => p.code == promoCode
                            && p.isActive == 'Y'
                            && p.startDate <= now
                            && p.endDate >= now);

                    if (promo != null)
                    {
                        promoCodeId = promo.PromoCodeID;
                        discountAmount = promo.DiscountType == "Percentage"
                            ? totalAmount * (promo.DiscountValue / 100)
                            : promo.DiscountValue;
                        discountAmount = Math.Min(discountAmount, totalAmount);
                        Console.WriteLine($"Promo applied: {promo.code}, Discount: {discountAmount}");
                    }
                }

                // FIXED: Create booking with all required fields including null values
                var booking = new BOOKING
                {
                    BookingReference = $"BKG-{Guid.NewGuid():N}".Substring(0, 12).ToUpper(),
                    member_id = appUser.member_id,
                    EventID = eventId,
                    BookingDateTime = DateTime.Now,
                    BookingStatus = "PendingPayment",
                    PaymentStatus = "Unpaid",
                    TotalAmount = totalAmount - discountAmount,
                    createdAt = DateTime.Now,
                    CancellationReason = null,
                    CancelledAt = null
                };

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    _context.BOOKING.Add(booking);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Booking created with ID: {booking.BookingID}");

                    foreach (var tt in ticketTypes)
                    {
                        var qty = selectedTickets[tt.TicketID];
                        var lineTotal = tt.Price * qty;

                        _context.BOOKING_ITEM.Add(new BOOKING_ITEM
                        {
                            BookingID = booking.BookingID,
                            TicketTypeID = tt.TicketID,
                            Quantity = qty,
                            UnitPrice = tt.Price,
                            LineTotal = lineTotal
                        });
                        Console.WriteLine($"Added booking item: {tt.TypeName} x{qty}");
                    }

                    if (promoCodeId.HasValue && discountAmount > 0)
                    {
                        _context.BOOKING_PROMO.Add(new BOOKING_PROMO
                        {
                            BookingID = booking.BookingID,
                            BookingCodeID = promoCodeId.Value,
                            DiscountedAmount = discountAmount,
                            AppliedAt = DateTime.Now
                        });
                        Console.WriteLine($"Added promo record");
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    Console.WriteLine($"Booking successful! ID: {booking.BookingID}");
                    TempData["Success"] = "Booking created successfully!";

                    return RedirectToAction("Confirmation", "BOOKINGs", new { id = booking.BookingID });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    var errorMessage = ex.Message;
                    if (ex.InnerException != null)
                    {
                        errorMessage += " | INNER: " + ex.InnerException.Message;

                        if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx)
                        {
                            errorMessage += $" | SQL Error {sqlEx.Number}: {sqlEx.Message}";
                        }
                    }

                    Console.WriteLine("========== DATABASE ERROR ==========");
                    Console.WriteLine(errorMessage);

                    TempData["Error"] = $"Failed to create booking: {errorMessage}";
                    return RedirectToAction(nameof(Book), new { id = eventId });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OUTER ERROR: {ex.Message}");
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id = eventId });
            }
        }

        // =========================
        // JOIN WAITING LIST
        // =========================
        [Authorize(Policy = "MemberOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> JoinWaitingList(int eventId)
        {
            var appUser = await GetCurrentAppUserAsync();
            if (appUser == null)
            {
                TempData["Error"] = "Cannot find your profile.";
                return RedirectToAction(nameof(Details), new { id = eventId });
            }

            var ev = await _context.EVENT.FirstOrDefaultAsync(e => e.eventID == eventId);
            if (ev == null) return NotFound();

            if (IsExternalMember() && !IsUniversityMember() && ev.visibility != "Public")
            {
                TempData["Error"] = "External members can only join public events.";
                return RedirectToAction(nameof(Details), new { id = eventId });
            }

            var alreadyInWaiting = await _context.WAITING_LIST.AnyAsync(w =>
                w.EventID == eventId &&
                w.member_id == appUser.member_id &&
                (w.Status == "Pending" || w.Status == "Active"));

            if (alreadyInWaiting)
            {
                TempData["Success"] = "You are already in the waiting list.";
                return RedirectToAction(nameof(Details), new { id = eventId });
            }

            _context.WAITING_LIST.Add(new WAITING_LIST
            {
                EventID = eventId,
                member_id = appUser.member_id,
                AddedAt = DateTime.Now,
                Status = "Pending"
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Added to waiting list.";
            return RedirectToAction(nameof(Details), new { id = eventId });
        }
    }
}