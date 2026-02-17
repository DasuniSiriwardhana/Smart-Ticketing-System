using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartTicketingSystem.Data;

namespace SmartTicketingSystem.Authorization
{
    public class HasAppRoleHandler : AuthorizationHandler<HasAppRoleRequirement>
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HasAppRoleHandler(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            HasAppRoleRequirement requirement)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
                return;

            // Identity user (logged in)
            var identityUser = await _userManager.GetUserAsync(context.User);
            if (identityUser == null)
                return;

            // Find your USER row by email (since your USER table uses member_id)
            var appUser = await _context.USER.FirstOrDefaultAsync(u => u.Email == identityUser.Email);
            if (appUser == null)
                return;

            // Get role names assigned to this member_id
            var userRoleNames = await (
                from ur in _context.USER_ROLE
                join r in _context.Role on ur.roleID equals r.RoleId
                where ur.member_id == appUser.member_id
                select r.rolename
            ).ToListAsync();

            // Check if user has ANY allowed role
            if (userRoleNames.Any(rn => requirement.AllowedRoles.Contains(rn)))
            {
                context.Succeed(requirement);
            }
        }
    }
}
