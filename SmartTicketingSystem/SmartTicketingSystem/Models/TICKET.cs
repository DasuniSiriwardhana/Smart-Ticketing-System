using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTicketingSystem.Models
{
    public class TICKET
    {
        [Key]
        public int TicketID { get; set; }

        [Required]
        public int BookingID { get; set; }

        [Required]
        [StringLength(500)]
        public string QRcodevalue { get; set; } = string.Empty;

        [Required]
        public DateTime issuedAt { get; set; }

        // Navigation Properties
        [ForeignKey("BookingID")]
        public virtual BOOKING? Booking { get; set; }
    }
}