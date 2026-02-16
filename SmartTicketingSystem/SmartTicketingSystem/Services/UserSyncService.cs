using Microsoft.EntityFrameworkCore;
using SmartTicketingSystem.Data;
using SmartTicketingSystem.Models;

namespace SmartTicketingSystem.Services
{
    public class UserSyncService
    {
        private readonly ApplicationDbContext _context;

        public UserSyncService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task EnsureUserExistsAsync(string email)
        {
            // Check if user already exists
            var existingUser = await _context.USER
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null)
                return;

            // Insert into USER table (no createdAt needed)
            var newUser = new USER
            {
                Email = email
            };

            _context.USER.Add(newUser);
            await _context.SaveChangesAsync();

            // Assign default role = ExternalMember
            var role = await _context.Role
                .FirstOrDefaultAsync(r => r.rolename == "ExternalMember");

            if (role == null)
                return;

            var userRole = new USER_ROLE
            {
                member_id = newUser.member_id,
                roleID = role.RoleId,
                AssignedAt = DateTime.Now
            };

            _context.USER_ROLE.Add(userRole);
            await _context.SaveChangesAsync();
        }
    }
}
