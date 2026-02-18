using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = "AdminOrOrganizer")]
public class OrganizerController : Controller
{
    public IActionResult Dashboard() => View();
}
