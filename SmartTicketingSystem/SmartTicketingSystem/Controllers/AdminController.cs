using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTicketingSystem.Data;
using SmartTicketingSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SmartTicketingSystem.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<USER?> GetCurrentAppUserAsync()
        {
            var identityId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(identityId)) return null;

            return await _context.USER.FirstOrDefaultAsync(u => u.IdentityUserId == identityId);
        }

        // =========================
        // DASHBOARD
        // =========================
        public IActionResult Dashboard()
        {
            ViewBag.TotalUsers = _context.USER.Count();
            ViewBag.TotalEvents = _context.EVENT.Count();
            ViewBag.TotalBookings = _context.BOOKING.Count();
            ViewBag.TotalPayments = _context.PAYMENT.Count();

            ViewBag.PendingApprovals = _context.EVENT.Count(e =>
                e.status == "PendingApproval" || e.status == "PendingUpcoming");

            ViewBag.PendingPublicRequests = _context.PUBLIC_EVENT_REQUEST.Count(p => p.status == "Pending");

            var bookingsPerMonth = _context.BOOKING
                .GroupBy(b => b.BookingDateTime.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .OrderBy(x => x.Month)
                .ToList();

            string[] monthNames = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

            ViewData["BookingMonthLabels"] = bookingsPerMonth
                .Select(x => monthNames[Math.Max(1, Math.Min(12, x.Month)) - 1])
                .ToList();

            ViewData["BookingCounts"] = bookingsPerMonth.Select(x => x.Count).ToList();

            var eventsByCategory = _context.EVENT
                .Join(_context.EVENT_CATEGORY,
                    e => e.categoryID,
                    c => c.categoryID,
                    (e, c) => new { c.categoryName })
                .GroupBy(x => x.categoryName)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            ViewData["CategoryLabels"] = eventsByCategory.Select(x => x.Category).ToList();
            ViewData["CategoryCounts"] = eventsByCategory.Select(x => x.Count).ToList();

            ViewData["LatestBookings"] = _context.BOOKING
                .Include(b => b.Event)
                .Include(b => b.User)
                .OrderByDescending(b => b.BookingDateTime)
                .Select(b => new {
                    b.BookingID,
                    b.BookingReference,
                    b.TotalAmount,
                    EventTitle = b.Event != null ? b.Event.title : "N/A",
                    UserEmail = b.User != null ? b.User.Email : "N/A"
                })
                .Take(5)
                .ToList();

            ViewData["LatestPayments"] = _context.PAYMENT
                .Include(p => p.Booking)
                .OrderByDescending(p => p.PaidAt)
                .Select(p => new {
                    p.PaymentID,
                    p.BookingID,
                    p.Amount,
                    BookingRef = p.Booking != null ? p.Booking.BookingReference : "N/A"
                })
                .Take(5)
                .ToList();

            ViewData["LatestRequests"] = _context.PUBLIC_EVENT_REQUEST
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new {
                    r.requestID,
                    r.requestFullName,
                    r.eventTitle,
                    r.status,
                    r.CreatedAt
                })
                .Take(5)
                .ToList();

            return View();
        }

        // QR SCANNER PAGE
        public IActionResult Scanner()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var todaysEvents = _context.EVENT
                .Where(e => e.StartDateTime.Date >= today && e.StartDateTime.Date < tomorrow)
                .OrderBy(e => e.StartDateTime)
                .ToList();

            ViewBag.TodaysEvents = todaysEvents;
            return View();
        }

        // VERIFY TICKET API
        [HttpPost]
        public async Task<IActionResult> VerifyTicket([FromBody] TicketScanRequest request)
        {
            if (string.IsNullOrEmpty(request.QRCode))
            {
                return Json(new { success = false, message = "Invalid QR code" });
            }

            var ticket = await _context.TICKET
                .Include(t => t.Booking)
                .ThenInclude(b => b.User)
                .Include(t => t.Booking.Event)
                .FirstOrDefaultAsync(t => t.QRcodevalue == request.QRCode);

            if (ticket == null)
            {
                return Json(new { success = false, message = "Ticket not found" });
            }

            if (request.EventId > 0 && ticket.Booking.EventID != request.EventId)
            {
                return Json(new
                {
                    success = false,
                    message = $"This ticket is for '{ticket.Booking.Event.title}', not the selected event"
                });
            }

            var existingAttendance = await _context.ATTENDANCE
                .FirstOrDefaultAsync(a => a.TicketID == ticket.TicketID);

            if (existingAttendance != null)
            {
                return Json(new
                {
                    success = false,
                    alreadyCheckedIn = true,
                    checkedInAt = existingAttendance.CheckedInAt.ToString("hh:mm tt, MMM dd"),
                    message = "Ticket already used for check-in",
                    ticket = new
                    {
                        ticket.TicketID,
                        EventName = ticket.Booking.Event.title,
                        AttendeeName = ticket.Booking.User?.FullName ?? "Unknown",
                        ticket.Booking.Event.StartDateTime
                    }
                });
            }

            return Json(new
            {
                success = true,
                ticket = new
                {
                    ticket.TicketID,
                    EventName = ticket.Booking.Event.title,
                    AttendeeName = ticket.Booking.User?.FullName ?? "Unknown",
                    AttendeeEmail = ticket.Booking.User?.Email ?? "",
                    EventDate = ticket.Booking.Event.StartDateTime.ToString("MMM dd, yyyy • hh:mm tt"),
                    ticket.issuedAt
                }
            });
        }

        // =========================
        // CONFIRM CHECK-IN
        // =========================
        [HttpPost]
        public async Task<IActionResult> ConfirmCheckIn([FromBody] CheckInRequest request)
        {
            try
            {
                var ticket = await _context.TICKET
                    .Include(t => t.Booking)
                    .FirstOrDefaultAsync(t => t.QRcodevalue == request.QRCode);

                if (ticket == null)
                {
                    return Json(new { success = false, message = "Ticket not found" });
                }

                var existingAttendance = await _context.ATTENDANCE
                    .AnyAsync(a => a.TicketID == ticket.TicketID);

                if (existingAttendance)
                {
                    return Json(new { success = false, message = "Already checked in" });
                }

                var attendance = new ATTENDANCE
                {
                    EventID = ticket.Booking.EventID,
                    member_id = ticket.Booking.member_id,
                    TicketID = ticket.TicketID,
                    CheckedInAt = DateTime.Now,
                    CheckInStatus = "Verified"
                };

                _context.ATTENDANCE.Add(attendance);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Check-in successful!",
                    checkedInAt = DateTime.Now.ToString("hh:mm:ss tt")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // =========================
        // MANAGE ATTENDANCE
        // =========================
        public async Task<IActionResult> ManageAttendance()
        {
            var events = await _context.EVENT
                .Where(e => e.status == "Published")
                .OrderByDescending(e => e.StartDateTime)
                .Select(e => new
                {
                    Event = e,
                    TotalBookings = _context.BOOKING.Count(b => b.EventID == e.eventID && b.PaymentStatus == "Paid"),
                    CheckedIn = _context.ATTENDANCE.Count(a => a.EventID == e.eventID),
                    EventDate = e.StartDateTime,
                    IsPast = e.endDateTime < DateTime.Now
                })
                .ToListAsync();

            return View(events);
        }

        // =========================
        // GET ATTENDEES FOR EVENT
        // =========================
        public async Task<IActionResult> EventAttendees(int eventId)
        {
            var ev = await _context.EVENT.FindAsync(eventId);
            if (ev == null) return NotFound();

            var bookings = await _context.BOOKING
                .Include(b => b.User)
                .Include(b => b.BookingItems)
                .ThenInclude(bi => bi.TicketType)
                .Where(b => b.EventID == eventId && b.PaymentStatus == "Paid")
                .ToListAsync();

            // Fixed: Changed ToHashSetAsync to ToListAsync + HashSet constructor
            var checkedInIds = await _context.ATTENDANCE
                .Where(a => a.EventID == eventId)
                .Select(a => a.member_id)
                .ToListAsync();

            var checkedInHashSet = new HashSet<int>(checkedInIds);

            var attendees = new List<AttendeeViewModel>();

            foreach (var booking in bookings)
            {
                var ticket = await _context.TICKET
                    .FirstOrDefaultAsync(t => t.BookingID == booking.BookingID);

                attendees.Add(new AttendeeViewModel
                {
                    BookingID = booking.BookingID,
                    UserID = booking.member_id,
                    UserName = booking.User?.FullName ?? "Unknown",
                    UserEmail = booking.User?.Email ?? "",
                    TicketID = ticket?.TicketID ?? 0,
                    QRCode = ticket?.QRcodevalue ?? "",
                    TicketType = booking.BookingItems?.FirstOrDefault()?.TicketType?.TypeName ?? "Standard",
                    IsCheckedIn = checkedInHashSet.Contains(booking.member_id),
                    CheckedInAt = checkedInHashSet.Contains(booking.member_id)
                        ? await _context.ATTENDANCE
                            .Where(a => a.EventID == eventId && a.member_id == booking.member_id)
                            .Select(a => (DateTime?)a.CheckedInAt)
                            .FirstOrDefaultAsync()
                        : null
                });
            }

            ViewBag.Event = ev;
            return View(attendees);
        }

        // =========================
        // MANUAL CHECK-IN
        // =========================
        [HttpPost]
        public async Task<IActionResult> ManualCheckIn([FromBody] ManualCheckInRequest request)
        {
            try
            {
                var existing = await _context.ATTENDANCE
                    .FirstOrDefaultAsync(a => a.EventID == request.EventId && a.member_id == request.UserId);

                if (existing != null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Already checked in at " + existing.CheckedInAt.ToString("hh:mm tt")
                    });
                }

                var ticket = await _context.TICKET
                    .FirstOrDefaultAsync(t => t.BookingID == request.BookingId);

                if (ticket == null)
                {
                    return Json(new { success = false, message = "Ticket not found" });
                }

                var attendance = new ATTENDANCE
                {
                    EventID = request.EventId,
                    member_id = request.UserId,
                    TicketID = ticket.TicketID,
                    CheckedInAt = DateTime.Now,
                    CheckInStatus = "Verified (Manual)"
                };

                _context.ATTENDANCE.Add(attendance);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Check-in successful!",
                    checkedInAt = DateTime.Now.ToString("hh:mm:ss tt")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // =========================
        // BULK CHECK-IN
        // =========================
        [HttpPost]
        public async Task<IActionResult> BulkCheckIn(int eventId, int[] userIds)
        {
            try
            {
                var tickets = await _context.TICKET
                    .Include(t => t.Booking)
                    .Where(t => t.Booking.EventID == eventId && userIds.Contains(t.Booking.member_id))
                    .ToDictionaryAsync(t => t.Booking.member_id);

                var checkedIn = 0;
                foreach (var userId in userIds)
                {
                    var existing = await _context.ATTENDANCE
                        .AnyAsync(a => a.EventID == eventId && a.member_id == userId);

                    if (existing) continue;

                    if (tickets.ContainsKey(userId))
                    {
                        _context.ATTENDANCE.Add(new ATTENDANCE
                        {
                            EventID = eventId,
                            member_id = userId,
                            TicketID = tickets[userId].TicketID,
                            CheckedInAt = DateTime.Now,
                            CheckInStatus = "Verified (Bulk)"
                        });
                        checkedIn++;
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"{checkedIn} attendees checked in successfully!";
                return RedirectToAction(nameof(EventAttendees), new { eventId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(EventAttendees), new { eventId });
            }
        }

        // =========================
        // ATTENDANCE REPORT
        // =========================
        public async Task<IActionResult> Attendance(int? eventId)
        {
            if (eventId == null)
            {
                var events = await _context.EVENT
                    .OrderByDescending(e => e.StartDateTime)
                    .Select(e => new
                    {
                        Event = e,
                        TotalBooked = _context.BOOKING.Count(b => b.EventID == e.eventID && b.PaymentStatus == "Paid"),
                        CheckedIn = _context.ATTENDANCE.Count(a => a.EventID == e.eventID)
                    })
                    .ToListAsync();

                return View(events);
            }
            else
            {
                var attendance = await (from a in _context.ATTENDANCE
                                        join u in _context.USER on a.member_id equals u.member_id
                                        join t in _context.TICKET on a.TicketID equals t.TicketID
                                        where a.EventID == eventId
                                        orderby a.CheckedInAt descending
                                        select new
                                        {
                                            a.AttendanceID,
                                            a.CheckedInAt,
                                            a.CheckInStatus,
                                            UserName = u.FullName,
                                            UserEmail = u.Email,
                                            t.QRcodevalue
                                        }).ToListAsync();

                var ev = await _context.EVENT.FindAsync(eventId);
                ViewBag.Event = ev;
                return View("EventAttendance", attendance);
            }
        }

        // =========================
        // APPROVALS LIST
        // =========================
        public async Task<IActionResult> Approvals()
        {
            var pending = await _context.EVENT
                .Where(e => e.status == "PendingApproval" || e.status == "PendingUpcoming")
                .OrderByDescending(e => e.createdAt)
                .ToListAsync();

            return View(pending);
        }

        // =========================
        // APPROVE EVENT
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveEvent(int id, string decisionNote)
        {
            var ev = await _context.EVENT.FirstOrDefaultAsync(e => e.eventID == id);
            if (ev == null) return NotFound();

            if (ev.status != "PendingApproval" && ev.status != "PendingUpcoming")
            {
                TempData["Error"] = "Only pending events can be approved.";
                return RedirectToAction(nameof(Approvals));
            }

            var adminUser = await GetCurrentAppUserAsync();
            if (adminUser == null)
            {
                TempData["Error"] = "Cannot find admin profile in USER table.";
                return RedirectToAction(nameof(Approvals));
            }

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
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

                if (!string.IsNullOrWhiteSpace(ev.visibility) &&
                    ev.visibility.Equals("Public", StringComparison.OrdinalIgnoreCase))
                {
                    PUBLIC_EVENT_REQUEST? linked = null;

                    linked = await _context.PUBLIC_EVENT_REQUEST
                        .Where(r => r.reviewedNote != null &&
                                   r.reviewedNote.Contains($"EVENT_ID:{ev.eventID}"))
                        .OrderByDescending(r => r.CreatedAt)
                        .FirstOrDefaultAsync();

                    if (linked == null)
                    {
                        linked = await _context.PUBLIC_EVENT_REQUEST
                            .Where(r => r.reviewedNote != null &&
                                       r.reviewedNote.Contains($"[LinkedEventID:{ev.eventID}]"))
                            .OrderByDescending(r => r.CreatedAt)
                            .FirstOrDefaultAsync();
                    }

                    if (linked != null)
                    {
                        linked.status = "Approved";
                        linked.ReviewedByUserID = adminUser.member_id;

                        if (!linked.reviewedNote.Contains($"EVENT_ID:{ev.eventID}"))
                        {
                            linked.reviewedNote = (linked.reviewedNote ?? "") +
                                                 $" | EVENT_ID:{ev.eventID} | APPROVED on {DateTime.Now}: {decisionNote}";
                        }
                        else
                        {
                            linked.reviewedNote = linked.reviewedNote +
                                                 $" | APPROVED on {DateTime.Now}: {decisionNote}";
                        }

                        _context.PUBLIC_EVENT_REQUEST.Update(linked);
                        await _context.SaveChangesAsync();
                    }
                }

                _context.EVENT.Update(ev);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["Success"] = "Event approved and published.";
                return RedirectToAction(nameof(Approvals));
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                TempData["Error"] = $"Approve failed: {ex.Message}";
                return RedirectToAction(nameof(Approvals));
            }
        }

        // =========================
        // REJECT EVENT
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectEvent(int id, string decisionNote)
        {
            var ev = await _context.EVENT.FirstOrDefaultAsync(e => e.eventID == id);
            if (ev == null) return NotFound();

            if (ev.status != "PendingApproval" && ev.status != "PendingUpcoming")
            {
                TempData["Error"] = "Only pending events can be rejected.";
                return RedirectToAction(nameof(Approvals));
            }

            var adminUser = await GetCurrentAppUserAsync();
            if (adminUser == null)
            {
                TempData["Error"] = "Cannot find admin profile in USER table.";
                return RedirectToAction(nameof(Approvals));
            }

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
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

                if (!string.IsNullOrWhiteSpace(ev.visibility) &&
                    ev.visibility.Equals("Public", StringComparison.OrdinalIgnoreCase))
                {
                    PUBLIC_EVENT_REQUEST? linked = null;

                    linked = await _context.PUBLIC_EVENT_REQUEST
                        .Where(r => r.reviewedNote != null &&
                                   r.reviewedNote.Contains($"EVENT_ID:{ev.eventID}"))
                        .OrderByDescending(r => r.CreatedAt)
                        .FirstOrDefaultAsync();

                    if (linked == null)
                    {
                        linked = await _context.PUBLIC_EVENT_REQUEST
                            .Where(r => r.reviewedNote != null &&
                                       r.reviewedNote.Contains($"[LinkedEventID:{ev.eventID}]"))
                            .OrderByDescending(r => r.CreatedAt)
                            .FirstOrDefaultAsync();
                    }

                    if (linked != null)
                    {
                        linked.status = "Rejected";
                        linked.ReviewedByUserID = adminUser.member_id;

                        if (!linked.reviewedNote.Contains($"EVENT_ID:{ev.eventID}"))
                        {
                            linked.reviewedNote = (linked.reviewedNote ?? "") +
                                                 $" | EVENT_ID:{ev.eventID} | REJECTED on {DateTime.Now}: {decisionNote}";
                        }
                        else
                        {
                            linked.reviewedNote = linked.reviewedNote +
                                                 $" | REJECTED on {DateTime.Now}: {decisionNote}";
                        }

                        _context.PUBLIC_EVENT_REQUEST.Update(linked);
                        await _context.SaveChangesAsync();
                    }
                }

                _context.EVENT.Update(ev);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["Success"] = "Event rejected.";
                return RedirectToAction(nameof(Approvals));
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                TempData["Error"] = $"Reject failed: {ex.Message}";
                return RedirectToAction(nameof(Approvals));
            }
        }

        // =========================
        // BOOKINGS
        // =========================
        public async Task<IActionResult> Bookings(string searchRef, string status, string payment)
        {
            try
            {
                var query = _context.BOOKING
                    .Include(b => b.Event)
                    .Include(b => b.User)
                    .Include(b => b.BookingItems)
                        .ThenInclude(bi => bi.TicketType)
                    .Include(b => b.Payments)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchRef))
                    query = query.Where(b => b.BookingReference.Contains(searchRef));

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(b => b.BookingStatus == status);

                if (!string.IsNullOrEmpty(payment))
                    query = query.Where(b => b.PaymentStatus == payment);

                var bookings = await query
                    .OrderByDescending(b => b.BookingDateTime)
                    .ToListAsync();

                ViewBag.TotalBookings = bookings.Count;
                ViewBag.PaidBookings = bookings.Count(b => b.PaymentStatus == "Paid");
                ViewBag.UnpaidBookings = bookings.Count(b => b.PaymentStatus == "Unpaid");
                ViewBag.TotalRevenue = bookings.Where(b => b.PaymentStatus == "Paid").Sum(b => b.TotalAmount);

                return View(bookings);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading bookings: {ex.Message}";
                return View(new List<BOOKING>());
            }
        }
    }

    // Request Models - Moved inside namespace
    public class TicketScanRequest
    {
        public string QRCode { get; set; }
        public int EventId { get; set; }
    }

    public class CheckInRequest
    {
        public string QRCode { get; set; }
    }

    public class ManualCheckInRequest
    {
        public int EventId { get; set; }
        public int UserId { get; set; }
        public int BookingId { get; set; }
    }

    public class AttendeeViewModel
    {
        public int BookingID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public int TicketID { get; set; }
        public string QRCode { get; set; }
        public string TicketType { get; set; }
        public bool IsCheckedIn { get; set; }
        public DateTime? CheckedInAt { get; set; }
    }
}