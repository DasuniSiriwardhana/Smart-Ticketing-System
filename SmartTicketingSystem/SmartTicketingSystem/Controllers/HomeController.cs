using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SmartTicketingSystem.Models;

namespace SmartTicketingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("Dashboard", "Admin");

                if (User.IsInRole("Organizer"))
                    return RedirectToAction("Dashboard", "Organizer");

                if (User.IsInRole("UniversityMember"))
                    return RedirectToAction("Dashboard", "UniversityMember");

                if (User.IsInRole("ExternalMember"))
                    return RedirectToAction("Dashboard", "ExternalMember");
            }

            return View(); // guest homepage
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
