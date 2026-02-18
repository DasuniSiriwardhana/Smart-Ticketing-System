using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = "AdminOrUniversityMember")]
public class UniversityMemberController : Controller
{
    public IActionResult Dashboard() => View();
}
