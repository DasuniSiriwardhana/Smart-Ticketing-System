#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
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

            // 1️⃣ Create Identity user
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

            // 2️⃣ Insert into YOUR USER table
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

            // 3️⃣ Assign role (SAFE way without EF expression error)

            var roles = await _context.Role.ToListAsync();

            string Normalize(string s)
            {
                return (s ?? "").Replace(" ", "").Trim().ToLower();
            }

            var desiredKey = Normalize(Input.UserType);

            var matchedRole = roles.FirstOrDefault(r =>
                Normalize(r.rolename) == desiredKey
            );

            // fallback to ExternalMember
            if (matchedRole == null)
            {
                matchedRole = roles.FirstOrDefault(r =>
                    Normalize(r.rolename) == "externalmember"
                );
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

            // 4️⃣ Sign in directly
            await _signInManager.SignInAsync(identityUser, isPersistent: false);

            return LocalRedirect(returnUrl);
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
