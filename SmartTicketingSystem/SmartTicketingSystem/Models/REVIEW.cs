using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;

namespace SmartTicketingSystem.Models
{
    public class REVIEW
    {
        [Key]
        public int ReviewID { get; set; }
        public int eventID { get; set; }
        public int userID { get; set; }
        public int Ratings { get; set; }
        public String Comments { get; set; }
        public char isVerifiedAttendee { get; set; }
        public string ReviewStatus { get; set; }
        public DateTime createdAt { get; set; }
    }
}
