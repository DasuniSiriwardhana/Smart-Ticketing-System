using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTicketingSystem.Models
{
    public class USER
    {
        [Key]
        public int member_id { get; set; }

        [StringLength(450)]
        public string? IdentityUserId { get; set; }  // Make nullable

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string phone { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string userType { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string UniversityNumber { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^[YN]$", ErrorMessage = "isverified must be Y or N.")]
        public char isverified { get; set; } = 'N';

        [NotMapped]
        public bool IsVerifiedBool
        {
            get => isverified == 'Y';
            set => isverified = value ? 'Y' : 'N';
        }

        [Required]
        [StringLength(20)]
        public string status { get; set; } = "Active";

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime createdAt { get; set; }

        public int? ApprovalID { get; set; }  // CHANGE: Make nullable!
    }
}