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

        public DbSet<EVENT> EVENT { get; set; } = default!;
        public DbSet<Role> Role { get; set; } = default!;
        public DbSet<USER_ROLE> USER_ROLE { get; set; } = default!;
        public DbSet<PUBLIC_EVENT_REQUEST> PUBLIC_EVENT_REQUEST { get; set; } = default!;
        public DbSet<TICKET> TICKET { get; set; } = default!;
        public DbSet<INQUIRY> INQUIRY { get; set; } = default!;
        public DbSet<EVENT_APPROVAL> EVENT_APPROVAL { get; set; } = default!;
        public DbSet<ATTENDANCE> ATTENDANCE { get; set; } = default!;
        public DbSet<REVIEW_REPORT> REVIEW_REPORT { get; set; } = default!;
        public DbSet<PAYMENT> PAYMENT { get; set; } = default!;
        public DbSet<USER> USER { get; set; } = default!;
        public DbSet<REVIEW> REVIEW { get; set; } = default!;
        public DbSet<BOOKING> BOOKING { get; set; } = default!;
        public DbSet<WAITING_LIST> WAITING_LIST { get; set; } = default!;
        public DbSet<BOOKING_ITEM> BOOKING_ITEM { get; set; } = default!;
        public DbSet<BOOKING_PROMO> BOOKING_PROMO { get; set; } = default!;
        public DbSet<ORGANIZER_UNIT> ORGANIZER_UNIT { get; set; } = default!;
        public DbSet<EVENT_CATEGORY> EVENT_CATEGORY { get; set; } = default!;
        public DbSet<TICKET_TYPE> TICKET_TYPE { get; set; } = default!;
        public DbSet<PROMO_CODE> PROMO_CODE { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<EVENT>()
                .HasOne<USER>()
                .WithMany()
                .HasForeignKey(e => e.createdByUserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EVENT>()
                .HasOne<EVENT_APPROVAL>()
                .WithMany()
                .HasForeignKey(e => e.ApprovalID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PUBLIC_EVENT_REQUEST>()
                .HasOne<USER>()
                .WithMany()
                .HasForeignKey(r => r.ReviewedByUserID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}