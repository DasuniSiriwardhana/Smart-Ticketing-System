using System.ComponentModel.DataAnnotations;

namespace SmartTicketingSystem.Models
{
    public class BOOKING
    {
        [Key]
        public int BookingID { get; set; }
        public string BookingReference { get; set; }
        public int member_id { get; set; }
        public int EventID { get; set; }
        public DateTime BookingDateTime { get; set; }
        public string BookingStatus { get; set; }
        [Range(0.1, 99999999)]

        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; }
        [StringLength(250)]
        public string CancellationReason { get; set; }
        public DateTime CancelledAt { get; set; }
        public DateTime createdAt { get; set; }

    }
}
