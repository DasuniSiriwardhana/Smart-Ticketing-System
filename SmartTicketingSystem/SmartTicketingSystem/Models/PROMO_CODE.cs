using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTicketingSystem.Models
{
    public class PROMO_CODE
    {
        [Key]
        public int PromoCodeID { get; set; }

        [Required]
        [StringLength(50)]
        public string code { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string DiscountType { get; set; } = string.Empty; // "Percentage" or "Fixed"

        [Required]
        [Range(0, 100)]
        public decimal DiscountValue { get; set; }

        [Required]
        public DateTime startDate { get; set; }

        [Required]
        public DateTime endDate { get; set; }

        [Required]
        [RegularExpression("^[YN]$", ErrorMessage = "isActive must be Y or N.")]
        public char isActive { get; set; } = 'Y'; // DB stores 'Y' or 'N'

        [Required]
        public DateTime createdAt { get; set; }

        // ===== ADD THIS NOTMAPPED PROPERTY FOR BOOLEAN USE =====
        [NotMapped]
        public bool IsActiveBool
        {
            get => isActive == 'Y';
            set => isActive = value ? 'Y' : 'N';
        }

        // Navigation Properties
        public virtual ICollection<BOOKING_PROMO> BookingPromos { get; set; } = new List<BOOKING_PROMO>();
    }
}