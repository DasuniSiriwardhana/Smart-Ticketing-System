using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTicketingSystem.Models
{
    public class BOOKING_PROMO
    {
        [Key]
        public int BookingPromoID { get; set; }
        public int BookingID { get; set; }
        public int BookingCodeID { get; set; }
        public decimal DiscountedAmount { get; set; }
        public DateTime AppliedAt { get; set; }

        // ===== ADD THESE NAVIGATION PROPERTIES =====
        [ForeignKey("BookingID")]
        public virtual BOOKING? Booking { get; set; }

        [ForeignKey("BookingCodeID")]
        public virtual PROMO_CODE? PromoCode { get; set; }
    }
}