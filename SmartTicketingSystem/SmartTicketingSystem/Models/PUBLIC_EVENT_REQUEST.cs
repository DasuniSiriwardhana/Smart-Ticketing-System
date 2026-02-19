using System.ComponentModel.DataAnnotations;

namespace SmartTicketingSystem.Models
{
    public class PUBLIC_EVENT_REQUEST
    {
        [Key]
        public int requestID { get; set; }
        public string requestFullName { get; set; } = string.Empty;
        public string RequestEmail { get; set; } = string.Empty;
        public string phoneNumber { get; set; } = string.Empty;
        public string eventTitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime proposedDateTime { get; set; }
        public string VenueorMode { get; set; } = string.Empty;
        public string status { get; set; } = "Pending";
        public string reviewedNote { get; set; } = string.Empty;
        public int? ReviewedByUserID { get; set; }  // CHANGE: Make nullable!
        public DateTime CreatedAt { get; set; }
    }
}