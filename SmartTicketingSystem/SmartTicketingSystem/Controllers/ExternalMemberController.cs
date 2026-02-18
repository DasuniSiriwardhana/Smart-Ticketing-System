using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = "ExternalMemberOnly")]
public class ExternalMemberController : Controller
{
    public IActionResult Dashboard() => View();
}
