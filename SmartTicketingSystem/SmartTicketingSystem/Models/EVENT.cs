using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTicketingSystem.Models
{
    public class EVENT
    {
        [Key]
        public int eventID { get; set; }

        [Required]
        [StringLength(120)]
        public string title { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime StartDateTime { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime endDateTime { get; set; }

        [Required]
        [StringLength(150)]
        public string venue { get; set; }

        [Required]
        [RegularExpression("^[YN]$", ErrorMessage = "IsOnline must be Y or N.")]
        public char IsOnline { get; set; }  // DB: 'Y' / 'N'

        [NotMapped]
        public bool IsOnlineBool
        {
            get => IsOnline == 'Y';
            set => IsOnline = value ? 'Y' : 'N';
        }

        [Url]
        [StringLength(300)]
        public string onlineLink { get; set; }

        [StringLength(300)]
        public string AccessibilityInfo { get; set; }

        [Range(1, 100000)]
        public int capacity { get; set; }

        [Required]
        [StringLength(30)]
        public string visibility { get; set; }

        [Required]
        [StringLength(30)]
        public string status { get; set; }

        [StringLength(200)]
        public string organizerInfo { get; set; }

        [StringLength(2000)]
        public string Agenda { get; set; }

        [Url]
        [StringLength(300)]
        public string maplink { get; set; }

        [Required]
        public int createdByUserID { get; set; }

        [Required]
        public int organizerUnitID { get; set; }

        [Required]
        public int categoryID { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime createdAt { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime updatedAt { get; set; }

        public int ApprovalID { get; set; }
    }
}
