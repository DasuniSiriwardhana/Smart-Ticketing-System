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
        public DbSet<SmartTicketingSystem.Models.REVIEW_REPORT> REVIEW_REPORT { get; set; } = default!;
        public DbSet<SmartTicketingSystem.Models.PAYMENT> PAYMENT { get; set; } = default!;
        public DbSet<SmartTicketingSystem.Models.USER> USER { get; set; } = default!;
        public DbSet<SmartTicketingSystem.Models.REVIEW> REVIEW { get; set; } = default!;
        public DbSet<SmartTicketingSystem.Models.BOOKING> BOOKING { get; set; } = default!;
        public DbSet<SmartTicketingSystem.Models.WAITING_LIST> WAITING_LIST { get; set; } = default!;
        public DbSet<SmartTicketingSystem.Models.BOOKING_ITEM> BOOKING_ITEM { get; set; } = default!;
        public DbSet<SmartTicketingSystem.Models.BOOKING_PROMO> BOOKING_PROMO { get; set; } = default!;
        public DbSet<SmartTicketingSystem.Models.ORGANIZER_UNIT> ORGANIZER_UNIT { get; set; } = default!;
    }
}
