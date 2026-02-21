using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTicketingSystem.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SmartTicketingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get featured events (upcoming published events)
            var featuredEvents = await _context.EVENT
                .Where(e => e.status == "Published" && e.StartDateTime > System.DateTime.Now)
                .OrderBy(e => e.StartDateTime)
                .Take(4)
                .ToListAsync();

            return View(featuredEvents);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Contact(string name, string email, string subject, string message)
        {
            // You can implement email sending here
            TempData["Success"] = "Thank you for contacting us. We'll respond soon!";
            return RedirectToAction(nameof(Index));
        }
    }
}