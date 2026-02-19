using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTicketingSystem.Data;
using SmartTicketingSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

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

    //  Dashboard
    public IActionResult Dashboard()
    {
        ViewBag.TotalUsers = _context.USER.Count();
        ViewBag.TotalEvents = _context.EVENT.Count();
        ViewBag.TotalBookings = _context.BOOKING.Count();
        ViewBag.TotalPayments = _context.PAYMENT.Count();

        // Pending approvals based on EVENT.status
        ViewBag.PendingApprovals = _context.EVENT.Count(e =>
            e.status == "PendingApproval" || e.status == "PendingUpcoming");

        ViewBag.PendingPublicRequests = _context.PUBLIC_EVENT_REQUEST.Count(p => p.status == "Pending");

        // Chart 1: bookings per month
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

        // Chart 2: events by category
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

        // Latest tables
        ViewData["LatestBookings"] = _context.BOOKING
            .OrderByDescending(b => b.BookingDateTime)
            .Select(b => new { b.BookingID, b.BookingReference, b.TotalAmount })
            .Take(5)
            .ToList();

        ViewData["LatestPayments"] = _context.PAYMENT
            .OrderByDescending(p => p.PaidAt)
            .Select(p => new { p.PaymentID, p.BookingID, p.Amount })
            .Take(5)
            .ToList();

        ViewData["LatestRequests"] = _context.PUBLIC_EVENT_REQUEST
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new { r.requestID, r.requestFullName, r.eventTitle, r.status, r.CreatedAt })
            .Take(5)
            .ToList();

        return View();
    }

    // Approvals list page
    public async Task<IActionResult> Approvals()
    {
        var pending = await _context.EVENT
            .Where(e => e.status == "PendingApproval" || e.status == "PendingUpcoming")
            .OrderByDescending(e => e.createdAt)
            .ToListAsync();

        return View(pending);
    }

    // Approve Event
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
            // 1) Insert approval record
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

            // 2) Update event
            ev.status = "Published";
            ev.updatedAt = DateTime.Now;
            ev.ApprovalID = approval.ApprovalID;

            // 3) If this was a public event, find and update the linked request record
            if (!string.IsNullOrWhiteSpace(ev.visibility) &&
                ev.visibility.Equals("Public", StringComparison.OrdinalIgnoreCase))
            {
                PUBLIC_EVENT_REQUEST? linked = null;

                // Strategy 1: Look for exact EVENT_ID in reviewedNote
                linked = await _context.PUBLIC_EVENT_REQUEST
                    .Where(r => r.reviewedNote != null &&
                               r.reviewedNote.Contains($"EVENT_ID:{ev.eventID}"))
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefaultAsync();

                // Strategy 2: Look for the old format
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

    // Reject Event - FIXED with all return paths
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
            // 1) Insert approval record
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

            // 2) Update event
            ev.status = "Rejected";
            ev.updatedAt = DateTime.Now;
            ev.ApprovalID = approval.ApprovalID;

            // 3) If this was a public event, find and update the linked request record
            if (!string.IsNullOrWhiteSpace(ev.visibility) &&
                ev.visibility.Equals("Public", StringComparison.OrdinalIgnoreCase))
            {
                PUBLIC_EVENT_REQUEST? linked = null;

                // Strategy 1: Look for exact EVENT_ID in reviewedNote
                linked = await _context.PUBLIC_EVENT_REQUEST
                    .Where(r => r.reviewedNote != null &&
                               r.reviewedNote.Contains($"EVENT_ID:{ev.eventID}"))
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefaultAsync();

                // Strategy 2: Look for the old format
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
}