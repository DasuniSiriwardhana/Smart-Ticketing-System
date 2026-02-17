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

            // Identity UserId
            var identityUserId = _userManager.GetUserId(context.User);
            if (string.IsNullOrWhiteSpace(identityUserId))
                return;

            // Find your USER row
            var appUser = await _context.USER
                .FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);

            if (appUser == null)
                return;

            // Check role mapping (USER_ROLE -> Role)
            var hasRole = await (
                from ur in _context.USER_ROLE
                join r in _context.Role on ur.roleID equals r.RoleId
                where ur.member_id == appUser.member_id
                      && r.rolename == requirement.RoleName
                select ur.UserRoleID
            ).AnyAsync();

            if (hasRole)
                context.Succeed(requirement);
        }
    }
}