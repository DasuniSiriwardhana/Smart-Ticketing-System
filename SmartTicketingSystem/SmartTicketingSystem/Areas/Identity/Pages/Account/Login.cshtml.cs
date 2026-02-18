// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace SmartTicketingSystem.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        // Decide where to go based on roles (priority)
        private string GetDashboardPath(IList<string> roles)
        {
            if (roles.Contains("Admin"))
                return "/Admin/Dashboard";

            if (roles.Contains("Organizer"))
                return "/Organizer/Dashboard";

            if (roles.Contains("UniversityMember"))
                return "/UniversityMember/Dashboard";

            if (roles.Contains("ExternalMember"))
                return "/ExternalMember/Dashboard";

            return "/Home/Index";
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            // If already logged in, do NOT show login page again
            if (User.Identity?.IsAuthenticated ?? false)
            {
                // We can read roles from the current user claims
                var roles = new List<string>();
                if (User.IsInRole("Admin")) roles.Add("Admin");
                if (User.IsInRole("Organizer")) roles.Add("Organizer");
                if (User.IsInRole("UniversityMember")) roles.Add("UniversityMember");
                if (User.IsInRole("ExternalMember")) roles.Add("ExternalMember");

                return Redirect(GetDashboardPath(roles));
            }

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(
                Input.Email,
                Input.Password,
                Input.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");

                // Get the logged-in user (Identity) and their roles
                var user = await _userManager.FindByEmailAsync(Input.Email);
                var roles = user != null
                    ? await _userManager.GetRolesAsync(user)
                    : new List<string>();

                // Optional: helpful logging while you test
                _logger.LogInformation("Roles for {Email}: {Roles}", Input.Email, string.Join(",", roles));

                // Redirect to correct dashboard (4 dashboards)
                var dashboardPath = GetDashboardPath(roles);
                return LocalRedirect(dashboardPath);
            }

            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return RedirectToPage("./Lockout");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }
    }
}
