using System.ComponentModel.DataAnnotations;

namespace SmartTicketingSystem.Models
{
    public class PAYMENT
    {
        [Key]
        public int PaymentID { get; set; }

        [Required]
        public int BookingID { get; set; }

        [Required]
        [StringLength(20)]
        public string PaymentType { get; set; }   

        [Required]
        [StringLength(30)]
        public string PaymentMethod { get; set; } 

        [StringLength(100)]
        public string TransactionReference { get; set; }

        [Range(0, 999999999)]
        public decimal Amount { get; set; }

        public DateTime PaidAt { get; set; }
    }
}
