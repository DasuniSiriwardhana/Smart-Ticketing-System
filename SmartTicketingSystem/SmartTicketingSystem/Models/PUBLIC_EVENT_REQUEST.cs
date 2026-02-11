using System.ComponentModel.DataAnnotations;

namespace SmartTicketingSystem.Models
{
    public class PUBLIC_EVENT_REQUEST
    {
        [Key]
        public int requestID { get; set; }
        public string requestFullName { get; set; }
        public string RequestEmail { get; set; }
        public string phoneNumber { get; set; }
        public string eventTitle { get; set; }
        public string Description { get; set; }
        public DateTime proposedDateTime { get; set; }
        public string VenueorMode { get; set; }
        public string status { get; set; }
        public string reviewedNote { get; set; }
        public int ReviewedByUserID { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
