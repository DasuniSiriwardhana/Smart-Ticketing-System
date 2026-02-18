using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTicketingSystem.Data;

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
        ViewBag.TotalUsers = _context.USER.Count();
        ViewBag.TotalEvents = _context.EVENT.Count();
        ViewBag.TotalBookings = _context.BOOKING.Count();
        ViewBag.TotalPayments = _context.PAYMENT.Count();
        ViewBag.PendingApprovals =_context.EVENT_APPROVAL.Count(e => e.Decision == 'P');
        ViewBag.PendingPublicRequests = _context.PUBLIC_EVENT_REQUEST.Count(p => p.status == "Pending");

        return View();
    }
}
