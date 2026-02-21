using System;

namespace SmartTicketingSystem.Models.ViewModels
{
    public class ReviewWithUserVM
    {
        // Review properties
        public int ReviewID { get; set; }
        public int eventID { get; set; }
        public int member_id { get; set; }
        public int Ratings { get; set; }
        public string Comments { get; set; } = string.Empty;
        public char isVerifiedAttendee { get; set; }
        public string ReviewStatus { get; set; } = string.Empty;
        public DateTime createdAt { get; set; }

        // User details (from JOIN)
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;

        // Event details (from JOIN)
        public string EventTitle { get; set; } = string.Empty;
    }
}