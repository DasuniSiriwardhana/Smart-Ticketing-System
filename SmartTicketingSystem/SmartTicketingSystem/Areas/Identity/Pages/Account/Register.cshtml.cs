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

            // IMPORTANT: This MUST match Identity role names you created:
            // Admin, Organizer, UniversityMember, ExternalMember
            [Required]
            [StringLength(30)]
            public string UserType { get; set; }

            [Required]
            [StringLength(30)]
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

            // 2) Insert into YOUR USER table
            var appUser = new USER
            {
                IdentityUserId = identityUser.Id,
                FullName = Input.FullName,
                Email = Input.Email,
                phone = Input.Phone,
                userType = Input.UserType,
                UniversityNumber = Input.UniversityNumber,
                isverified = 'N',
                status = "Active",
                createdAt = DateTime.Now,
                ApprovalID = 0
            };

            _context.USER.Add(appUser);
            await _context.SaveChangesAsync();

            // 3) Assign Identity role (THIS FIXES AspNetUserRoles)
            // Normalize Input.UserType to one of your Identity roles
            string NormalizeKey(string s) => (s ?? "").Replace(" ", "").Trim().ToLower();

            var key = NormalizeKey(Input.UserType);

            // Map common inputs to exact Identity role names
            string identityRole =
                key == "admin" ? "Admin" :
                key == "organizer" ? "Organizer" :
                key == "universitymember" ? "UniversityMember" :
                key == "externalmember" ? "ExternalMember" :
                "ExternalMember"; // fallback

            // IMPORTANT: roles must exist already (your seeding creates them)
            await _userManager.AddToRoleAsync(identityUser, identityRole);

            // 4) (Optional) Keep your custom USER_ROLE table in sync
            // If your custom Role table uses different names (like "Sports Club President"),
            // this will only match when rolename equals the selected user type.
            var roles = await _context.Role.ToListAsync();

            string Normalize(string s) => (s ?? "").Replace(" ", "").Trim().ToLower();
            var desiredKey = Normalize(Input.UserType);

            var matchedRole = roles.FirstOrDefault(r => Normalize(r.rolename) == desiredKey);

            // fallback to ExternalMember (only works if your custom Role table has it)
            if (matchedRole == null)
            {
                matchedRole = roles.FirstOrDefault(r => Normalize(r.rolename) == "externalmember");
            }

            if (matchedRole != null)
            {
                _context.USER_ROLE.Add(new USER_ROLE
                {
                    member_id = appUser.member_id,
                    roleID = matchedRole.RoleId,
                    AssignedAt = DateTime.Now
                });

                await _context.SaveChangesAsync();
            }

            // 5) Sign in directly
            await _signInManager.SignInAsync(identityUser, isPersistent: false);

            return LocalRedirect(returnUrl);
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
