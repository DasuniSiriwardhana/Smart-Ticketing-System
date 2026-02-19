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

        private bool IsUniversityOrganizer() => IsOrganizer() && IsUniversityMember();
        private bool IsExternalOrganizer() => IsOrganizer() && IsExternalMember();

        // =========================
        // INDEX (Upcoming events list with role rules)
        // =========================
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;

            var baseQuery = _context.EVENT
                .Where(e => e.StartDateTime >= now)
                .AsQueryable();

            // Guest: Published + Public only
            if (!User.Identity.IsAuthenticated)
            {
                var guest = await baseQuery
                    .Where(e => e.status == "Published" && e.visibility == "Public")
                    .OrderBy(e => e.StartDateTime)
                    .ToListAsync();

                return View(guest);
            }

            // Admin: everything upcoming
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

            // External Member (not Uni): Published Public + own events (any status) if organizer
            if (IsExternalMember() && !IsUniversityMember())
            {
                var q = baseQuery.Where(e =>
                    (e.status == "Published" && e.visibility == "Public")
                    || (IsOrganizer() && appUser != null && e.createdByUserID == appUser.member_id)
                );

                var list = await q.OrderBy(e => e.StartDateTime).ToListAsync();
                return View(list);
            }

            // University Member: Published (Public + University) + PendingUpcoming
            // + own events (any status) if organizer
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

            // Fallback
            var fallback = await baseQuery
                .Where(e => e.status == "Published" && e.visibility == "Public")
                .OrderBy(e => e.StartDateTime)
                .ToListAsync();

            return View(fallback);
        }

        // =========================
        // DETAILS (Role protected)
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.EVENT.FirstOrDefaultAsync(e => e.eventID == id.Value);
            if (ev == null) return NotFound();

            // Guest: only Published Public
            if (!User.Identity.IsAuthenticated)
            {
                if (!(ev.status == "Published" && ev.visibility == "Public"))
                    return Forbid();
            }
            else
            {
                // External Member: Published Public only, unless it's their own organizer event
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

                // University Member: Published or PendingUpcoming, unless it's their own organizer event
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

            // Ticket types (active + within sales window)
            var now = DateTime.Now;
            var ticketTypes = await _context.TICKET_TYPE
                .Where(t => t.EventID == ev.eventID
                            && t.isActive == 'Y'
                            && t.salesStartAt <= now
                            && t.salesEndAt >= now)
                .OrderBy(t => t.Price)
                .ToListAsync();

            // Booked seats per ticket type
            var bookedByTicketType = await _context.BOOKING_ITEM
                .Join(_context.TICKET_TYPE,
                    bi => bi.TicketTypeID,
                    tt => tt.TicketID,
                    (bi, tt) => new { bi, tt })
                .Where(x => x.tt.EventID == ev.eventID)
                .GroupBy(x => x.tt.TicketID)
                .Select(g => new { TicketID = g.Key, Qty = g.Sum(z => z.bi.Quantity) })
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
        // CREATE (POST) - UPDATED WITH RELIABLE LINKING AND NULL HANDLING
        // =========================
        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("title,Description,StartDateTime,endDateTime,venue,IsOnlineBool,onlineLink,AccessibilityInfo,capacity,visibility,organizerInfo,Agenda,maplink,organizerUnitID,categoryID")]
            EVENT ev)
        {
            // 1) Re-load dropdowns if validation fails
            if (!ModelState.IsValid)
            {
                await LoadDropDownsAsync(ev.categoryID, ev.organizerUnitID);
                return View(ev);
            }

            // 2) Must have logged app user
            var identityId = _userManager.GetUserId(User);
            var appUser = await _context.USER.FirstOrDefaultAsync(u => u.IdentityUserId == identityId);
            if (appUser == null)
            {
                ModelState.AddModelError("", "Cannot find your profile (USER table).");
                await LoadDropDownsAsync(ev.categoryID, ev.organizerUnitID);
                return View(ev);
            }

            // 3) Convert checkbox bool -> char 'Y'/'N'
            ev.IsOnline = ev.IsOnlineBool ? 'Y' : 'N';

            // 4) Enforce online link rules server-side
            if (ev.IsOnline == 'N')
                ev.onlineLink = null;

            // 5) Visibility: only Public or University
            if (!string.Equals(ev.visibility, "Public", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(ev.visibility, "University", StringComparison.OrdinalIgnoreCase))
            {
                ev.visibility = "University";
            }

            // 6) Force system fields (never user input)
            ev.createdByUserID = appUser.member_id;
            ev.createdAt = DateTime.Now;
            ev.updatedAt = DateTime.Now;

            // Status: submit for admin approval directly
            ev.status = "PendingApproval";
            ev.ApprovalID = 0;

            // Ensure non-nullable string fields have values
            ev.title = ev.title ?? "";
            ev.Description = ev.Description ?? "";
            ev.venue = ev.venue ?? "";
            ev.onlineLink = ev.onlineLink ?? "";
            ev.AccessibilityInfo = ev.AccessibilityInfo ?? "";
            ev.organizerInfo = ev.organizerInfo ?? "";
            ev.Agenda = ev.Agenda ?? "";
            ev.maplink = ev.maplink ?? "";
            ev.visibility = ev.visibility ?? "University";
            ev.status = ev.status ?? "PendingApproval";

            // Start a transaction to ensure both records are created or none
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.EVENT.Add(ev);
                await _context.SaveChangesAsync();

                // If Public event -> also create a Public Event Request record with RELIABLE linking
                if (!string.IsNullOrWhiteSpace(ev.visibility) &&
                    ev.visibility.Equals("Public", StringComparison.OrdinalIgnoreCase))
                {
                    // Create a unique token for linking
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
                        // Store both Event ID and token in reviewedNote for reliable linking
                        reviewedNote = $"EVENT_ID:{ev.eventID}|TOKEN:{linkToken}|Submitted by organizer on {DateTime.Now}"
                    };

                    _context.PUBLIC_EVENT_REQUEST.Add(req);
                    await _context.SaveChangesAsync();

                    // Optional: Update event with token for cross-reference
                    ev.organizerInfo = (ev.organizerInfo ?? "") + $" [RequestToken:{linkToken}]";
                    _context.EVENT.Update(ev);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                TempData["Success"] = "Event submitted for admin approval.";
                return RedirectToAction(nameof(Details), new { id = ev.eventID });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Error creating event. Please try again.");
                await LoadDropDownsAsync(ev.categoryID, ev.organizerUnitID);
                return View(ev);
            }
        }

        // =========================
        // ADMIN: Pending Approvals Queue
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
        // ADMIN: Approve Event
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
                TempData["Error"] = "Cannot find admin profile in USER table.";
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
        // ADMIN: Reject Event
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
                TempData["Error"] = "Cannot find admin profile in USER table.";
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
        // BOOK NOW
        // =========================
        [Authorize(Policy = "MemberOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookNow(int eventId, Dictionary<int, int> quantities)
        {
            if (quantities == null || quantities.Count == 0)
            {
                TempData["Error"] = "Please select at least one ticket quantity.";
                return RedirectToAction(nameof(Details), new { id = eventId });
            }

            var selected = quantities
                .Where(kv => kv.Value > 0)
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            if (selected.Count == 0)
            {
                TempData["Error"] = "Please select at least one ticket quantity.";
                return RedirectToAction(nameof(Details), new { id = eventId });
            }

            var appUser = await GetCurrentAppUserAsync();
            if (appUser == null)
            {
                TempData["Error"] = "Cannot find your profile. Please contact admin.";
                return RedirectToAction(nameof(Details), new { id = eventId });
            }

            var ev = await _context.EVENT.FirstOrDefaultAsync(e => e.eventID == eventId);
            if (ev == null) return NotFound();

            if (!string.Equals(ev.status, "Published", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "This event is not available for booking.";
                return RedirectToAction(nameof(Details), new { id = eventId });
            }

            // External member can only book Public events
            if (IsExternalMember() && !IsUniversityMember() &&
                !string.Equals(ev.visibility, "Public", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "External members can only book public events.";
                return RedirectToAction(nameof(Details), new { id = eventId });
            }

            var ticketIds = selected.Keys.ToList();
            var now = DateTime.Now;

            var ticketTypes = await _context.TICKET_TYPE
                .Where(t => t.EventID == eventId
                            && ticketIds.Contains(t.TicketID)
                            && t.isActive == 'Y'
                            && t.salesStartAt <= now
                            && t.salesEndAt >= now)
                .ToListAsync();

            if (ticketTypes.Count != ticketIds.Count)
            {
                TempData["Error"] = "One or more selected ticket types are invalid or not available.";
                return RedirectToAction(nameof(Details), new { id = eventId });
            }

            // How many seats already booked per ticket type
            var bookedByTT = await _context.BOOKING_ITEM
                .Join(_context.TICKET_TYPE,
                    bi => bi.TicketTypeID,
                    tt => tt.TicketID,
                    (bi, tt) => new { bi, tt })
                .Where(x => x.tt.EventID == eventId)
                .GroupBy(x => x.tt.TicketID)
                .Select(g => new { TicketID = g.Key, Qty = g.Sum(z => z.bi.Quantity) })
                .ToListAsync();

            var bookedMap = bookedByTT.ToDictionary(x => x.TicketID, x => x.Qty);

            foreach (var tt in ticketTypes)
            {
                var alreadyBooked = bookedMap.ContainsKey(tt.TicketID) ? bookedMap[tt.TicketID] : 0;
                var remaining = Math.Max(0, tt.seatLimit - alreadyBooked);
                var want = selected[tt.TicketID];

                if (want > remaining)
                {
                    TempData["Error"] = $"Not enough seats for {tt.TypeName}. Remaining: {remaining}.";
                    return RedirectToAction(nameof(Details), new { id = eventId });
                }
            }

            // Check total event capacity
            var totalAlreadyBooked = bookedByTT.Sum(x => x.Qty);
            var totalWant = selected.Values.Sum();

            if (totalAlreadyBooked + totalWant > ev.capacity)
            {
                TempData["Error"] = "Event is full. Please join the waiting list.";
                return RedirectToAction(nameof(Details), new { id = eventId });
            }

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var booking = new BOOKING
                {
                    BookingReference = $"BKG-{Guid.NewGuid():N}".Substring(0, 12).ToUpper(),
                    member_id = appUser.member_id,
                    EventID = eventId,
                    BookingDateTime = DateTime.Now,
                    BookingStatus = "PendingPayment",
                    PaymentStatus = "Unpaid",
                    TotalAmount = 0,
                    createdAt = DateTime.Now
                };

                _context.BOOKING.Add(booking);
                await _context.SaveChangesAsync();

                decimal total = 0;
                foreach (var tt in ticketTypes)
                {
                    var qty = selected[tt.TicketID];
                    var lineTotal = tt.Price * qty;
                    total += lineTotal;

                    _context.BOOKING_ITEM.Add(new BOOKING_ITEM
                    {
                        BookingID = booking.BookingID,
                        TicketTypeID = tt.TicketID,
                        Quantity = qty,
                        UnitPrice = tt.Price,
                        LineTotal = lineTotal
                    });
                }

                booking.TotalAmount = total;
                _context.BOOKING.Update(booking);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["Success"] = "Booking created. Choose Pay Now or Pay Later.";
                return RedirectToAction("Confirmation", "BOOKINGs", new { id = booking.BookingID });
            }
            catch
            {
                await tx.RollbackAsync();
                TempData["Error"] = "Booking failed. Please try again.";
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
                TempData["Error"] = "Cannot find your profile. Please contact admin.";
                return RedirectToAction(nameof(Details), new { id = eventId });
            }

            var ev = await _context.EVENT.FirstOrDefaultAsync(e => e.eventID == eventId);
            if (ev == null) return NotFound();

            // External can only join waiting list for public events
            if (IsExternalMember() && !IsUniversityMember() &&
                !string.Equals(ev.visibility, "Public", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "External members can only request public events.";
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

            TempData["Success"] = "You have been added to the waiting list.";
            return RedirectToAction(nameof(Details), new { id = eventId });
        }
    }
}