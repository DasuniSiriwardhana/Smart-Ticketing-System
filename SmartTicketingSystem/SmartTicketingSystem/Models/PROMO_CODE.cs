using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTicketingSystem.Models
{
    public class PROMO_CODE
    {
        [Key]
        public int PromoCodeID { get; set; }

        [Required]
        [StringLength(30)]
        public string code { get; set; }

        [Required]
        [StringLength(20)]
        public string DiscountType { get; set; } 

        [Required]
        [Range(0.01, 999999)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime startDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime endDate { get; set; }

        // setting checkboxes for this 
        [Required]
        [RegularExpression("^[YN]$", ErrorMessage = "isActive must be Y or N.")]
        public char isActive { get; set; } 

        [NotMapped]
        public bool IsActiveBool
        {
            get => isActive == 'Y';
            set => isActive = value ? 'Y' : 'N';
        }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime createdAt { get; set; }
    }
}
