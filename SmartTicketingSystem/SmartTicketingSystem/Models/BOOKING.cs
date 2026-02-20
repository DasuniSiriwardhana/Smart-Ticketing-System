using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTicketingSystem.Models
{
    public class BOOKING
    {
        [Key]
        public int BookingID { get; set; }

        [Required]
        [StringLength(20)]
        public string BookingReference { get; set; } = string.Empty;

        [Required]
        public int member_id { get; set; }

        [Required]
        public int EventID { get; set; }

        [Required]
        public DateTime BookingDateTime { get; set; }

        [Required]
        [StringLength(30)]
        public string BookingStatus { get; set; } = "PendingPayment";

        [Range(0.1, 99999999)]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(20)]
        public string PaymentStatus { get; set; } = "Unpaid";

        [StringLength(250)]
        public string? CancellationReason { get; set; }

        public DateTime? CancelledAt { get; set; }

        [Required]
        public DateTime createdAt { get; set; }

        // Navigation Properties
        [ForeignKey("member_id")]
        public virtual USER? User { get; set; }

        [ForeignKey("EventID")]
        public virtual EVENT? Event { get; set; }

        public virtual ICollection<BOOKING_ITEM> BookingItems { get; set; } = new List<BOOKING_ITEM>();

        public virtual ICollection<PAYMENT> Payments { get; set; } = new List<PAYMENT>();

        public virtual BOOKING_PROMO? BookingPromo { get; set; }

        public virtual ICollection<TICKET> Tickets { get; set; } = new List<TICKET>();
    }
}