using System.ComponentModel.DataAnnotations;

namespace SmartTicketingSystem.Models
{
    public class TICKET_TYPE
    {
        [Key]
        public int TicketID { get; set; }
        public int EventID { get; set; }
        public string TypeName { get; set; }
        public decimal Price { get; set; }
        public int seatLimit { get; set; }
        public DateTime salesStartAt { get; set; }
        public DateTime salesEndAt { get; set ; }
        public char isActive { get; set; }
        public DateTime createdAt { get; set; }

    }
}
