using System.ComponentModel.DataAnnotations;

namespace SmartTicketingSystem.Models
{
    public class EVENT_APPROVAL
    {
        [Key]
        public int ApprovalID { get; set; }
        public int EventID { get; set; }
        public int ApprovedByUserID { get; set; }
        public char Decision { get; set; }
        public string DecisionNote { get; set; } = string.Empty;  // Default to empty string
        public DateTime DecisionDateTime { get; set; }
        public int member_id { get; set; }
    }
}