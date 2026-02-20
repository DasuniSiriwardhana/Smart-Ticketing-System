using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTicketingSystem.Models
{
    public class BOOKING_ITEM
    {
        [Key]
        public int BookingItemID { get; set; }
        public int BookingID { get; set; }
        public int TicketTypeID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }

        // ===== ADD THESE NAVIGATION PROPERTIES =====
        [ForeignKey("BookingID")]
        public virtual BOOKING? Booking { get; set; }

        [ForeignKey("TicketTypeID")]
        public virtual TICKET_TYPE? TicketType { get; set; }
    }
}