using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTicketingSystem.Models
{
    public class ORGANIZER_UNIT
    {
        [Key]
        public int OrganizerID { get; set; }

        [Required]
        [StringLength(100)]
        public string unitTime { get; set; }

        [Required]
        [StringLength(80)]
        public string UnitType { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string ContactEmail { get; set; }

        [Required]
        [StringLength(20)]
        public string ContactPhone { get; set; }

        [Required]
        [RegularExpression("^[YN]$", ErrorMessage = "status must be Y or N.")]
        public char status { get; set; }  // DB: 'Y' / 'N'

        [NotMapped]
        public bool StatusBool
        {
            get => status == 'Y';
            set => status = value ? 'Y' : 'N';
        }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }
    }
}
