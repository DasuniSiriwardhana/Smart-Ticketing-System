using System.ComponentModel.DataAnnotations;

namespace SmartTicketingSystem.Models
{
    public class REVIEW_REPORT
    {
        [Key]
        public int RportID { get; set; }
        public int ReviewID { get; set; }
        public int ReportedByUserID { get; set; }
        public string ReportReason { get; set; }
        public string ReportDetail { get; set; }
        public DateTime ReportedAt { get; set; }

    }
}
