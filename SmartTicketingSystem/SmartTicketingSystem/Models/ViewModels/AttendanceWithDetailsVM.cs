namespace SmartTicketingSystem.Models.ViewModels
{
    public class AttendanceWithDetailsVM
    {
        // Attendance properties
        public int AttendanceID { get; set; }
        public int EventID { get; set; }
        public int member_id { get; set; }
        public int TicketID { get; set; }
        public DateTime CheckedInAt { get; set; }
        public string CheckInStatus { get; set; } = string.Empty;

        // User details (from JOIN)
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;

        // Ticket details (from JOIN)
        public string TicketCode { get; set; } = string.Empty;
        public string TicketTypeName { get; set; } = string.Empty;
    }
}