using System.ComponentModel.DataAnnotations;

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


    }
}
