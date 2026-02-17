using Microsoft.AspNetCore.Authorization;
namespace SmartTicketingSystem.Authorization
{
    public class HasAppRoleRequirement : IAuthorizationRequirement
    {
        public string RoleName { get; }
        public HasAppRoleRequirement(string roleName) => RoleName = roleName;
    }
}
