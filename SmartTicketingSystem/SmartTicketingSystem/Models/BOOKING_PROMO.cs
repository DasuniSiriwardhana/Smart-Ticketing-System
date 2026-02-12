using System.ComponentModel.DataAnnotations;

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

    }
}
