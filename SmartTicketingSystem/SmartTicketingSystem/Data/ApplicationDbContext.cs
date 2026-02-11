using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartTicketingSystem.Models;

namespace SmartTicketingSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<SmartTicketingSystem.Models.EVENT> EVENT { get; set; } = default!;
        public DbSet<SmartTicketingSystem.Models.Role> Role { get; set; } = default!;
        public DbSet<SmartTicketingSystem.Models.USER_ROLE> USER_ROLE { get; set; } = default!;
        public DbSet<SmartTicketingSystem.Models.PUBLIC_EVENT_REQUEST> PUBLIC_EVENT_REQUEST { get; set; } = default!;
        public DbSet<SmartTicketingSystem.Models.TICKET> TICKET { get; set; } = default!;
        public DbSet<SmartTicketingSystem.Models.INQUIRY> INQUIRY { get; set; } = default!;
        public DbSet<SmartTicketingSystem.Models.EVENT_APPROVAL> EVENT_APPROVAL { get; set; } = default!;
        public DbSet<SmartTicketingSystem.Models.ATTENDANCE> ATTENDANCE { get; set; } = default!;
    }
}
