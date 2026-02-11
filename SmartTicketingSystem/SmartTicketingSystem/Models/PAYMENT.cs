using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTicketingSystem.Models
{
    public class PAYMENT
    {
        [Key]
        public int PaymentID { get; set; }
        public int BookingID { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionReference { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; } 
    }
}
