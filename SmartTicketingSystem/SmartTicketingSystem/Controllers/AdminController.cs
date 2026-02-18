using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartTicketingSystem.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        public IActionResult Dashboard() => View();

        public IActionResult Users() => View();
        public IActionResult Events() => View();
        public IActionResult Bookings() => View();
        public IActionResult Tickets() => View();
        public IActionResult Payments() => View();
        public IActionResult Approvals() => View();
        public IActionResult Promotions() => View();
        public IActionResult Inquiries() => View();
        public IActionResult Reviews() => View();
        public IActionResult Reports() => View();
    }
}