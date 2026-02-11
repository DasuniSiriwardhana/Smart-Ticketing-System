using System.ComponentModel.DataAnnotations;

namespace SmartTicketingSystem.Models
{
    public class TICKET
    {
        [Key]
        public int TicketID { get; set; }
        public int BookingID { get; set; }
        public string QRcodevalue { get; set; }
        public DateTime issuedAt { get; set; }
    }
}
