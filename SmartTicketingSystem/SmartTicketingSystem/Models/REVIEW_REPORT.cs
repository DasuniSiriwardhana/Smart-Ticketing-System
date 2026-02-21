using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTicketingSystem.Models
{
    public class REVIEW_REPORT
    {
        [Key]
        public int ReportID { get; set; }  // Fixed typo from ReportID

        [Required]
        public int ReviewID { get; set; }

        [Required]
        public int ReportedByUserID { get; set; }

        [Required]
        [StringLength(100)]
        public string ReportReason { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ReportDetail { get; set; }

        public DateTime ReportedAt { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string? ReportStatus { get; set; } = "Pending";

        public int? ReviewedByAdminID { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [StringLength(500)]
        public string? AdminNotes { get; set; }

        // Navigation properties (not mapped to DB)
        [ForeignKey("ReviewID")]
        public virtual REVIEW? Review { get; set; }

        [ForeignKey("ReportedByUserID")]
        public virtual USER? Reporter { get; set; }
    }
}