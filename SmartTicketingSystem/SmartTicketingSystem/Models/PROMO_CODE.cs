using System.ComponentModel.DataAnnotations;

namespace SmartTicketingSystem.Models
{
    public class PROMO_CODE
    {
        [Key]
        public int PromoCodeID { get; set; }
        public string code { get; set; }
        public string DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public char isActive { get; set; }
        public DateTime createdAt { get; set; }

    }
}
