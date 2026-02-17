using Microsoft.AspNetCore.Authorization;

namespace SmartTicketingSystem.Authorization
{
    public class HasAppRoleRequirement : IAuthorizationRequirement
    {
        public string[] AllowedRoles { get; }

        public HasAppRoleRequirement(params string[] allowedRoles)
        {
            AllowedRoles = allowedRoles;
        }
    }
}
