using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTicketingSystem.Models
{
    public class TICKET_TYPE
    {
        [Key]
        public int TicketID { get; set; } // (later we can rename to TicketTypeID)

        [Required]
        public int EventID { get; set; }

        [Required]
        [StringLength(60)]
        public string TypeName { get; set; }

        [Required]
        [Range(0.00, 99999999)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Range(0, 100000)]
        public int seatLimit { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime salesStartAt { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime salesEndAt { get; set; }

        [Required]
        [RegularExpression("^[YN]$", ErrorMessage = "isActive must be Y or N.")]
        public char isActive { get; set; } // DB: 'Y' / 'N'

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
