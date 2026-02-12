using System.ComponentModel.DataAnnotations;

namespace SmartTicketingSystem.Models
{
    public class ORGANIZER_UNIT
    {
        [Key]
        public int OrganizerID { get; set; }
        public string unitTime { get; set; }
        public string UnitType { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public char status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
