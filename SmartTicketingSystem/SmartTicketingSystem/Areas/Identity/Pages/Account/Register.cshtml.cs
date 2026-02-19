#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartTicketingSystem.Data;
using SmartTicketingSystem.Models;

namespace SmartTicketingSystem.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(100)]
            public string FullName { get; set; }

            [Required]
            [StringLength(20)]
            public string Phone { get; set; }

            // Must match Identity role names: Organizer, UniversityMember, ExternalMember
            [Required]
            [StringLength(30)]
            public string UserType { get; set; }

            // NOT required globally (required conditionally below)
            [StringLength(30)]
            [RegularExpression(@"^E.*$", ErrorMessage = "University Number must start with 'E' (example: E12345).")]
            public string UniversityNumber { get; set; }


            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [StringLength(100, MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Compare("Password")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager
                .GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
                return Page();

            // ✅ Conditional validation:
            var userType = (Input.UserType ?? "").Trim();
            bool isUniversityMember = string.Equals(userType, "UniversityMember", StringComparison.OrdinalIgnoreCase);
            bool isOrganizer = string.Equals(userType, "Organizer", StringComparison.OrdinalIgnoreCase);
            bool hasUniNumber = !string.IsNullOrWhiteSpace(Input.UniversityNumber);

            if (isUniversityMember && !hasUniNumber)
            {
                ModelState.AddModelError("Input.UniversityNumber", "University Number is required for university members.");
                return Page();
            }

            // 1) Create Identity user
            var identityUser = new IdentityUser();
            await _userStore.SetUserNameAsync(identityUser, Input.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(identityUser, Input.Email, CancellationToken.None);

            var result = await _userManager.CreateAsync(identityUser, Input.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return Page();
            }

            _logger.LogInformation("Identity user created.");

            // 2) Assign Identity roles (this controls dashboards + policies)
            if (isOrganizer)
            {
                // Everyone selecting Organizer gets Organizer role
                await _userManager.AddToRoleAsync(identityUser, "Organizer");

                // Organizer split:
                // If has UniversityNumber => UniversityOrganizer (Organizer + UniversityMember)
                // Else => ExternalOrganizer (Organizer + ExternalMember)
                if (hasUniNumber)
                    await _userManager.AddToRoleAsync(identityUser, "UniversityMember");
                else
                    await _userManager.AddToRoleAsync(identityUser, "ExternalMember");
            }
            else
            {
                // ExternalMember or UniversityMember
                if (string.Equals(userType, "ExternalMember", StringComparison.OrdinalIgnoreCase))
                    await _userManager.AddToRoleAsync(identityUser, "ExternalMember");
                else if (string.Equals(userType, "UniversityMember", StringComparison.OrdinalIgnoreCase))
                    await _userManager.AddToRoleAsync(identityUser, "UniversityMember");
                else
                    await _userManager.AddToRoleAsync(identityUser, "ExternalMember"); // fallback
            }

            // 3) Insert into YOUR USER table
            // Store userType meaningfully:
            // - Organizer stays Organizer
            // - Member roles stay as selected
            var appUser = new USER
            {
                IdentityUserId = identityUser.Id,
                FullName = Input.FullName,
                Email = Input.Email,
                phone = Input.Phone,
                userType = userType,
                UniversityNumber = hasUniNumber ? Input.UniversityNumber : null,
                isverified = 'N',
                status = "Active",
                createdAt = DateTime.Now,
                ApprovalID = 0
            };

            _context.USER.Add(appUser);
            await _context.SaveChangesAsync();

            // 4) Sign in
            await _signInManager.SignInAsync(identityUser, isPersistent: false);

            return LocalRedirect(returnUrl);
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
