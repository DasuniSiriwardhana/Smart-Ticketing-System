using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTicketingSystem.Data;
using System;
using System.Linq;

[Authorize(Policy = "AdminOnly")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Dashboard()
    {
        // ===== Summary cards =====
        ViewBag.TotalUsers = _context.USER.Count();
        ViewBag.TotalEvents = _context.EVENT.Count();
        ViewBag.TotalBookings = _context.BOOKING.Count();
        ViewBag.TotalPayments = _context.PAYMENT.Count();

        ViewBag.PendingApprovals = _context.EVENT_APPROVAL.Count(e => e.Decision == 'P');
        ViewBag.PendingPublicRequests = _context.PUBLIC_EVENT_REQUEST.Count(p => p.status == "Pending");

        // ===== Chart 1: Bookings per month (month labels) =====
        var bookingsPerMonth = _context.BOOKING
            .GroupBy(b => b.BookingDateTime.Month)
            .Select(g => new { Month = g.Key, Count = g.Count() })
            .OrderBy(x => x.Month)
            .ToList();

        string[] monthNames = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        var bookingMonthLabels = bookingsPerMonth
            .Select(x => monthNames[Math.Max(1, Math.Min(12, x.Month)) - 1])
            .ToList();

        var bookingCounts = bookingsPerMonth.Select(x => x.Count).ToList();

        ViewData["BookingMonthLabels"] = bookingMonthLabels;
        ViewData["BookingCounts"] = bookingCounts;

        // ===== Chart 2: Events per category (pie) =====
        // Works even if there are no events/categories
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

        // ===== Latest tables (Top 5) =====
        // Latest Bookings
        var latestBookings = _context.BOOKING
            .OrderByDescending(b => b.BookingDateTime)
            .Select(b => new
            {
                b.BookingID,
                b.BookingReference,
                b.TotalAmount,
                b.BookingStatus,
                b.BookingDateTime
            })
            .Take(5)
            .ToList();

        // Latest Payments
        var latestPayments = _context.PAYMENT
            .OrderByDescending(p => p.PaidAt)
            .Select(p => new
            {
                p.PaymentID,
                p.BookingID,
                p.Amount,
                p.PaymentMethod,
                p.PaidAt
            })
            .Take(5)
            .ToList();

        // Latest Public Requests
        var latestRequests = _context.PUBLIC_EVENT_REQUEST
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.requestID,
                r.requestFullName,
                r.eventTitle,
                r.status,
                r.CreatedAt
            })
            .Take(5)
            .ToList();

        ViewData["LatestBookings"] = latestBookings;
        ViewData["LatestPayments"] = latestPayments;
        ViewData["LatestRequests"] = latestRequests;

        return View();
    }
}
