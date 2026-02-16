using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTicketingSystem.Models
{
    public class REVIEW
    {
        [Key]
        public int ReviewID { get; set; }

        [Required]
        public int eventID { get; set; }

        [Required]
        public int member_id { get; set; }

        [Required]
        [Range(1, 5)]
        public int Ratings { get; set; }

        [StringLength(500)]
        public string Comments { get; set; }

        [Required]
        [RegularExpression("^[YN]$", ErrorMessage = "isVerifiedAttendee must be Y or N.")]
        public char isVerifiedAttendee { get; set; } // DB: 'Y' / 'N'

        [NotMapped]
        public bool IsVerifiedAttendeeBool
        {
            get => isVerifiedAttendee == 'Y';
            set => isVerifiedAttendee = value ? 'Y' : 'N';
        }

        [Required]
        [StringLength(20)]
        public string ReviewStatus { get; set; } // Pending/Approved/Rejected

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime createdAt { get; set; }
    }
}
