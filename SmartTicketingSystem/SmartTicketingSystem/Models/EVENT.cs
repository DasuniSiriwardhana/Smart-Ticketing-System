using System.ComponentModel.DataAnnotations;

namespace SmartTicketingSystem.Models
{
    public class EVENT
    {
        public int eventID { get; set; }
        [Required]
        public string title { get; set; }
        public string Description { get; set; }
        [Required]
        public DateTime StartDateTime { get; set; }
        public DateTime endDateTime { get; set; }
        [Required]
        public string venue { get; set; }
        [Required]
        public char IsOnline { get; set; }
        public string onlineLink { get; set; }
        public string AccessibilityInfo { get; set; }
        public int capacity { get; set; }
        public string visibility { get; set; }
        public string status { get; set; }
        public string organizerInfo { get; set; }
        public string Agenda { get; set; }
        public string maplink { get; set; }
        public int createdByUserID { get; set; }
        public int organizerUnitID { get; set; }
        public int categoryID { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
        public int ApprovalID { get; set; }
    }
}
