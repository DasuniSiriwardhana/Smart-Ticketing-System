using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTicketingSystem.Models
{
    public class EVENT : IValidatableObject
    {
        [Key]
        public int eventID { get; set; }

        [Required]
        [StringLength(120)]
        public string title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime StartDateTime { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime endDateTime { get; set; }

        [Required]
        [StringLength(150)]
        public string venue { get; set; } = string.Empty;

        [RegularExpression("^[YN]$", ErrorMessage = "IsOnline must be Y or N.")]
        public char IsOnline { get; set; } = 'N';  // Default to 'N'

        [NotMapped]
        public bool IsOnlineBool
        {
            get => IsOnline == 'Y';
            set => IsOnline = value ? 'Y' : 'N';
        }

        [Url]
        [StringLength(300)]
        public string? onlineLink { get; set; }  // Make nullable

        [StringLength(300)]
        public string? AccessibilityInfo { get; set; }  // Make nullable

        [Range(1, 100000)]
        public int capacity { get; set; }

        [Required]
        [StringLength(30)]
        public string visibility { get; set; } = "University";

        [StringLength(30)]
        public string status { get; set; } = "PendingApproval";

        [StringLength(200)]
        public string? organizerInfo { get; set; }  // Make nullable

        [StringLength(2000)]
        public string? Agenda { get; set; }  // Make nullable

        [Url]
        [StringLength(300)]
        public string? maplink { get; set; }  // Make nullable

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

        public int? ApprovalID { get; set; }  // CHANGE: Make nullable!

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsOnlineBool)
            {
                if (string.IsNullOrWhiteSpace(onlineLink))
                {
                    yield return new ValidationResult(
                        "Online link is required when the event is online.",
                        new[] { nameof(onlineLink) }
                    );
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(onlineLink))
                {
                    yield return new ValidationResult(
                        "Online link should be empty when the event is not online.",
                        new[] { nameof(onlineLink) }
                    );
                }
            }
            if (endDateTime != default && endDateTime < StartDateTime)
            {
                yield return new ValidationResult(
                    "End date/time must be after Start date/time.",
                    new[] { nameof(endDateTime) }
                );
            }
        }
    }
}