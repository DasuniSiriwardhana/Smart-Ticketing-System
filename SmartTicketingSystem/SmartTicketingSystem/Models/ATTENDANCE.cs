using System.ComponentModel.DataAnnotations;

namespace SmartTicketingSystem.Models
{
    public class ATTENDANCE
    {
        [Key]
        public int AttendanceID { get; set; }
        public int EventID { get; set; }
        public int UserID { get; set; }
        public int TicketID { get; set; }
        public DateTime CheckedInAt { get; set; }
        public string CheckInStatus { get; set; }
    }
}
