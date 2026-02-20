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

            // BOOKING relationships
            modelBuilder.Entity<BOOKING>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.member_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BOOKING>()
                .HasOne(b => b.Event)
                .WithMany()
                .HasForeignKey(b => b.EventID)
                .OnDelete(DeleteBehavior.Restrict);

            // BOOKING_ITEM relationships
            modelBuilder.Entity<BOOKING_ITEM>()
                .HasOne(bi => bi.Booking)
                .WithMany(b => b.BookingItems)
                .HasForeignKey(bi => bi.BookingID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BOOKING_ITEM>()
                .HasOne(bi => bi.TicketType)
                .WithMany(tt => tt.BookingItems)
                .HasForeignKey(bi => bi.TicketTypeID)
                .OnDelete(DeleteBehavior.Restrict);

            // BOOKING_PROMO relationships
            modelBuilder.Entity<BOOKING_PROMO>()
                .HasOne(bp => bp.Booking)
                .WithOne(b => b.BookingPromo)
                .HasForeignKey<BOOKING_PROMO>(bp => bp.BookingID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BOOKING_PROMO>()
                .HasOne(bp => bp.PromoCode)
                .WithMany(pc => pc.BookingPromos)
                .HasForeignKey(bp => bp.BookingCodeID)
                .OnDelete(DeleteBehavior.Restrict);

            // PAYMENT relationships
            modelBuilder.Entity<PAYMENT>()
                .HasOne(p => p.Booking)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BookingID)
                .OnDelete(DeleteBehavior.Restrict);

            // TICKET relationships
            modelBuilder.Entity<TICKET>()
                .HasOne(t => t.Booking)
                .WithMany()
                .HasForeignKey(t => t.BookingID)
                .OnDelete(DeleteBehavior.Cascade);

            // WAITING_LIST relationships
            modelBuilder.Entity<WAITING_LIST>()
                .HasOne(w => w.Event)
                .WithMany()
                .HasForeignKey(w => w.EventID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WAITING_LIST>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.member_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TICKET>()
      .HasOne(t => t.Booking)
      .WithMany(b => b.Tickets)
      .HasForeignKey(t => t.BookingID)
      .OnDelete(DeleteBehavior.Cascade)
      .HasConstraintName("FK_TICKET_BOOKING_BookingID"); // Explicit naming[citation:7]

            modelBuilder.Entity<PAYMENT>()
                .HasOne(p => p.Booking)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BookingID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_PAYMENT_BOOKING_BookingID"); // Explicit naming
        }
    }
}