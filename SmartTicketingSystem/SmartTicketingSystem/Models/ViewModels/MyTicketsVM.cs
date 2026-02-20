namespace SmartTicketingSystem.Models.ViewModels
{
    public class MyTicketsVM
    {
        public List<TicketGroupVM> UpcomingTickets { get; set; } = new();
        public List<TicketGroupVM> PastTickets { get; set; } = new();
        public TicketStatsVM Stats { get; set; } = new();
    }

    public class TicketGroupVM
    {
        public int BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string EventTitle { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string EventVenue { get; set; } = string.Empty;
        public List<TicketVM> Tickets { get; set; } = new();
    }

    public class TicketVM
    {
        public int TicketId { get; set; }
        public string QRCodeValue { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public string QRCodeImage => $"/TICKETs/Qr/{TicketId}";
    }

    public class TicketStatsVM
    {
        public int TotalTickets { get; set; }
        public int UpcomingEvents { get; set; }
        public int PastEvents { get; set; }
    }
}