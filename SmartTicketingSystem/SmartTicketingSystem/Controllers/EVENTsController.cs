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
using System.IO;

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

        // Helpers
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

        private string GetEventImageUrl(int eventId)
        {
            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/events");
                if (!Directory.Exists(uploadsFolder))
                    return null;

                var files = Directory.GetFiles(uploadsFolder, $"event_{eventId}.*");
                if (files.Length > 0)
                {
                    var fileName = Path.GetFileName(files[0]);
                    return $"/uploads/events/{fileName}";
                }
            }
            catch { }

            return null;
        }

        private bool IsAdmin() => User.IsInRole("Admin");
        private bool IsOrganizer() => User.IsInRole("Organizer");
        private bool IsUniversityMember() => User.IsInRole("UniversityMember");
        private bool IsExternalMember() => User.IsInRole("ExternalMember");

        // INDEX
        [AllowAnonymous]
        public async Task<IActionResult> Index(string search, string category, string date, decimal? minPrice, decimal? maxPrice, string location, bool? online, bool? free, string sort)
        {
            var now = DateTime.Now;

            var baseQuery = _context.EVENT
                .Where(e => e.StartDateTime >= now && e.status == "Published")
                .AsQueryable();

            if (!User.Identity.IsAuthenticated)
            {
                baseQuery = baseQuery.Where(e => e.visibility == "Public");
            }

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
                baseQuery = baseQuery.Where(e => e.title.Contains(search) || (e.Description ?? "").Contains(search));

            // Apply category filter
            if (!string.IsNullOrEmpty(category))
                baseQuery = baseQuery.Where(e => e.categoryID == _context.EVENT_CATEGORY
                    .FirstOrDefault(c => c.categoryName == category).categoryID);

            // Apply date filters
            if (!string.IsNullOrEmpty(date))
            {
                var today = DateTime.Today;
                switch (date)
                {
                    case "today":
                        baseQuery = baseQuery.Where(e => e.StartDateTime.Date == today);
                        break;
                    case "tomorrow":
                        baseQuery = baseQuery.Where(e => e.StartDateTime.Date == today.AddDays(1));
                        break;
                    case "week":
                        var endOfWeek = today.AddDays(7);
                        baseQuery = baseQuery.Where(e => e.StartDateTime >= today && e.StartDateTime <= endOfWeek);
                        break;
                    case "month":
                        var endOfMonth = today.AddMonths(1);
                        baseQuery = baseQuery.Where(e => e.StartDateTime >= today && e.StartDateTime <= endOfMonth);
                        break;
                }
            }

            // Apply location filter
            if (!string.IsNullOrEmpty(location))
                baseQuery = baseQuery.Where(e => e.venue.Contains(location));

            // Apply online filter
            if (online.HasValue && online.Value)
                baseQuery = baseQuery.Where(e => e.IsOnline == 'Y');

            var events = await baseQuery.OrderBy(e => e.StartDateTime).ToListAsync();

            // Filter by price in memory (since it requires joining with TicketTypes)
            if (minPrice.HasValue)
                events = events.Where(e => _context.TICKET_TYPE.Any(t => t.EventID == e.eventID && t.Price >= minPrice)).ToList();
            if (maxPrice.HasValue)
                events = events.Where(e => _context.TICKET_TYPE.Any(t => t.EventID == e.eventID && t.Price <= maxPrice)).ToList();
            if (free.HasValue && free.Value)
                events = events.Where(e => _context.TICKET_TYPE.Any(t => t.EventID == e.eventID && t.Price == 0)).ToList();

            // Apply sorting
            events = sort switch
            {
                "price_asc" => events.OrderBy(e => _context.TICKET_TYPE.Where(t => t.EventID == e.eventID).Min(t => (decimal?)t.Price) ?? 0).ToList(),
                "price_desc" => events.OrderByDescending(e => _context.TICKET_TYPE.Where(t => t.EventID == e.eventID).Min(t => (decimal?)t.Price) ?? 0).ToList(),
                "name" => events.OrderBy(e => e.title).ToList(),
                _ => events.OrderBy(e => e.StartDateTime).ToList()
            };

            ViewBag.Categories = await _context.EVENT_CATEGORY.ToListAsync();
            return View(events);
        }


        // DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ev = await _context.EVENT.FirstOrDefaultAsync(e => e.eventID == id.Value);
            if (ev == null) return NotFound();

            // Permission checks
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

            // Get reviews for this event
            var reviews = await (from r in _context.REVIEW
                                 join u in _context.USER on r.member_id equals u.member_id
                                 where r.eventID == ev.eventID && r.ReviewStatus == "Approved"
                                 orderby r.createdAt descending
                                 select new
                                 {
                                     r.ReviewID,
                                     r.Ratings,
                                     r.Comments,
                                     r.createdAt,
                                     UserFullName = u.FullName ?? "Anonymous"
                                 }).Take(5).ToListAsync();

            ViewBag.Reviews = reviews;
            ViewBag.TotalReviews = await _context.REVIEW.CountAsync(r => r.eventID == ev.eventID && r.ReviewStatus == "Approved");
            ViewBag.AverageRating = reviews.Any() ? reviews.Average(r => r.Ratings) : 0;

            // Check if current user can review
            if (User.Identity.IsAuthenticated)
            {
                var currentUser = await GetCurrentAppUserAsync();
                if (currentUser != null)
                {
                    var hasPaidBooking = await _context.BOOKING
                        .AnyAsync(b => b.member_id == currentUser.member_id &&
                                      b.EventID == ev.eventID &&
                                      b.PaymentStatus == "Paid");

                    var eventEnded = ev.endDateTime < DateTime.Now;
                    var alreadyReviewed = await _context.REVIEW
                        .AnyAsync(r => r.member_id == currentUser.member_id && r.eventID == ev.eventID);

                    ViewBag.CanReview = hasPaidBooking && eventEnded && !alreadyReviewed;

                    if (alreadyReviewed)
                    {
                        ViewBag.UserReview = await _context.REVIEW
                            .FirstOrDefaultAsync(r => r.member_id == currentUser.member_id && r.eventID == ev.eventID);
                    }

                    ViewBag.OnWaitingList = await _context.WAITING_LIST
                        .AnyAsync(w => w.member_id == currentUser.member_id &&
                                      w.EventID == ev.eventID &&
                                      w.Status == "Pending");
                }
            }

            // Get active promotions
            var activePromos = await _context.PROMO_CODE
                .Where(p => p.isActive == 'Y'
                            && p.startDate <= now
                            && p.endDate >= now)
                .ToListAsync();

            var usedPromoIds = await _context.BOOKING_PROMO
                .Where(bp => bp.Booking != null && bp.Booking.EventID == ev.eventID)
                .Select(bp => bp.BookingCodeID)
                .Distinct()
                .ToListAsync();

            activePromos = activePromos
                .Where(p => !usedPromoIds.Contains(p.PromoCodeID))
                .ToList();

            ViewBag.ActivePromos = activePromos;
            ViewBag.EventImageUrl = GetEventImageUrl(ev.eventID);

            var vm = new EventDetailsVM
            {
                Event = ev,
                TicketTypes = ticketTypes,
                RemainingByTicketType = remainingByTT,
                RemainingEventCapacity = remainingEventCap
            };

            return View(vm);
        }

        // CREATE (GET)
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

        // CREATE (POST) WITH IMAGE UPLOAD
        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("title,Description,StartDateTime,endDateTime,venue,IsOnlineBool,onlineLink,AccessibilityInfo,capacity,visibility,organizerInfo,Agenda,maplink,organizerUnitID,categoryID")]
            EVENT ev, IFormFile? eventImage)
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

                // Handle image upload
                if (eventImage != null && eventImage.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/events");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    // Delete any existing image for this event
                    var existingFiles = Directory.GetFiles(uploadsFolder, $"event_{ev.eventID}.*");
                    foreach (var file in existingFiles)
                    {
                        System.IO.File.Delete(file);
                    }

                    // Save new image
                    var fileExtension = Path.GetExtension(eventImage.FileName);
                    var fileName = $"event_{ev.eventID}{fileExtension}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await eventImage.CopyToAsync(fileStream);
                    }
                }

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
        // EDIT (GET)
        // =========================
        [Authorize(Policy = "AdminOrOrganizer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ev = await _context.EVENT.FindAsync(id);
            if (ev == null)
            {
                return NotFound();
            }

            // Check if user has permission to edit this event
            if (!User.IsInRole("Admin"))
            {
                var currentUser = await GetCurrentAppUserAsync();
                if (currentUser == null || ev.createdByUserID != currentUser.member_id)
                {
                    return Forbid();
                }
            }

            // Convert char to bool for the view
            ev.IsOnlineBool = ev.IsOnline == 'Y';

            await LoadDropDownsAsync(ev.categoryID, ev.organizerUnitID);
            return View(ev);
        }

        // =========================
        // EDIT (POST)
        // =========================
        [Authorize(Policy = "AdminOrOrganizer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("eventID,title,Description,StartDateTime,endDateTime,venue,IsOnlineBool,onlineLink,AccessibilityInfo,capacity,visibility,organizerInfo,Agenda,maplink,organizerUnitID,categoryID")]
            EVENT ev, IFormFile? eventImage)
        {
            if (id != ev.eventID)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await LoadDropDownsAsync(ev.categoryID, ev.organizerUnitID);
                return View(ev);
            }

            // Check if user has permission to edit this event
            if (!User.IsInRole("Admin"))
            {
                var currentUser = await GetCurrentAppUserAsync();
                var existingEvent = await _context.EVENT.AsNoTracking().FirstOrDefaultAsync(e => e.eventID == id);
                if (currentUser == null || existingEvent == null || existingEvent.createdByUserID != currentUser.member_id)
                {
                    return Forbid();
                }
            }

            ev.IsOnline = ev.IsOnlineBool ? 'Y' : 'N';
            if (ev.IsOnline == 'N')
                ev.onlineLink = null;

            ev.updatedAt = DateTime.Now;

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
                _context.Update(ev);
                await _context.SaveChangesAsync();

                // Handle image upload
                if (eventImage != null && eventImage.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/events");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    // Delete any existing image for this event
                    var existingFiles = Directory.GetFiles(uploadsFolder, $"event_{ev.eventID}.*");
                    foreach (var file in existingFiles)
                    {
                        System.IO.File.Delete(file);
                    }

                    // Save new image
                    var fileExtension = Path.GetExtension(eventImage.FileName);
                    var fileName = $"event_{ev.eventID}{fileExtension}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await eventImage.CopyToAsync(fileStream);
                    }
                }

                await transaction.CommitAsync();
                TempData["Success"] = "Event updated successfully.";
                return RedirectToAction(nameof(Details), new { id = ev.eventID });
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                if (!EventExists(ev.eventID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Error updating event.");
                await LoadDropDownsAsync(ev.categoryID, ev.organizerUnitID);
                return View(ev);
            }
        }

        // =========================
        // Helper for Edit
        // =========================
        private bool EventExists(int id)
        {
            return _context.EVENT.Any(e => e.eventID == id);
        }

        // PENDING APPROVALS
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
        // BOOK (GET)
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
                    .Include(bi => bi.Booking)
                    .Where(bi => bi.Booking != null && bi.Booking.EventID == ev.eventID && bi.Booking.PaymentStatus == "Paid")
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
                        TypeName = tt.TypeName ?? "",
                        Price = tt.Price,
                        AvailableQuantity = Math.Max(0, tt.seatLimit - booked),
                        SeatLimit = tt.seatLimit
                    });
                }

                // Get available promos for this event
                var usedPromoIds = await _context.BOOKING_PROMO
                    .Include(bp => bp.Booking)
                    .Where(bp => bp.Booking != null && bp.Booking.EventID == ev.eventID)
                    .Select(bp => bp.BookingCodeID)
                    .Distinct()
                    .ToListAsync();

                var availablePromos = await _context.PROMO_CODE
                    .Where(p => p.isActive == 'Y' && p.startDate <= now && p.endDate >= now)
                    .ToListAsync();

                ViewBag.EventPromos = availablePromos
                    .Where(p => !usedPromoIds.Contains(p.PromoCodeID))
                    .ToList();

                ViewBag.Event = ev;
                ViewBag.EventId = id;
                ViewBag.EventImageUrl = GetEventImageUrl(ev.eventID);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // =========================
        // BOOK (POST)
        // =========================
        [Authorize(Policy = "MemberOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(int eventId, Dictionary<int, int> quantities, string? promoCode = null)
        {
            try
            {
                if (quantities == null || !quantities.Any(kv => kv.Value > 0))
                {
                    TempData["Error"] = "Please select at least one ticket.";
                    return RedirectToAction(nameof(Book), new { id = eventId });
                }

                var selectedTickets = quantities
                    .Where(kv => kv.Value > 0)
                    .ToDictionary(kv => kv.Key, kv => kv.Value);

                var appUser = await GetCurrentAppUserAsync();
                if (appUser == null)
                {
                    TempData["Error"] = "Please login to book tickets.";
                    return RedirectToAction("Login", "Account");
                }

                var ev = await _context.EVENT
                    .FirstOrDefaultAsync(e => e.eventID == eventId && e.status == "Published");

                if (ev == null)
                {
                    TempData["Error"] = "Event not found.";
                    return RedirectToAction(nameof(Index));
                }

                if (IsExternalMember() && !IsUniversityMember() && ev.visibility != "Public")
                {
                    TempData["Error"] = "External members can only book public events.";
                    return RedirectToAction(nameof(Details), new { id = eventId });
                }

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
                    TempData["Error"] = "One or more selected tickets are not available.";
                    return RedirectToAction(nameof(Book), new { id = eventId });
                }

                // Check availability
                var bookedSeats = await _context.BOOKING_ITEM
                    .Include(bi => bi.Booking)
                    .Where(bi => bi.Booking != null && bi.Booking.EventID == eventId && bi.Booking.PaymentStatus == "Paid")
                    .GroupBy(bi => bi.TicketTypeID)
                    .Select(g => new { TicketTypeID = g.Key, Booked = g.Sum(bi => bi.Quantity) })
                    .ToDictionaryAsync(x => x.TicketTypeID, x => x.Booked);

                foreach (var tt in ticketTypes)
                {
                    bookedSeats.TryGetValue(tt.TicketID, out int booked);
                    var available = tt.seatLimit - booked;

                    if (selectedTickets[tt.TicketID] > available)
                    {
                        TempData["Error"] = $"Only {available} {tt.TypeName} tickets left.";
                        return RedirectToAction(nameof(Book), new { id = eventId });
                    }
                }

                // Check total capacity
                var totalBooked = await _context.BOOKING_ITEM
                    .Include(bi => bi.Booking)
                    .Where(bi => bi.Booking != null && bi.Booking.EventID == eventId && bi.Booking.PaymentStatus == "Paid")
                    .SumAsync(bi => (int?)bi.Quantity) ?? 0;

                var totalRequested = selectedTickets.Values.Sum();

                if (totalBooked + totalRequested > ev.capacity)
                {
                    TempData["Info"] = "This event is full. Would you like to join the waiting list?";
                    return RedirectToAction("Join", "WAITING_LIST", new { eventId });
                }

                // Calculate total
                decimal totalAmount = selectedTickets.Sum(kv =>
                    ticketTypes.First(tt => tt.TicketID == kv.Key).Price * kv.Value);

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
                    }
                }

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
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["Success"] = "Booking created successfully!";
                    return RedirectToAction("Confirmation", "BOOKINGs", new { id = booking.BookingID });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = $"Failed to create booking: {ex.Message}";
                    return RedirectToAction(nameof(Book), new { id = eventId });
                }
            }
            catch (Exception ex)
            {
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
                w.Status == "Pending");

            if (alreadyInWaiting)
            {
                TempData["Info"] = "You are already in the waiting list.";
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