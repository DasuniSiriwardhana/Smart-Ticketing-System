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
    }
}
