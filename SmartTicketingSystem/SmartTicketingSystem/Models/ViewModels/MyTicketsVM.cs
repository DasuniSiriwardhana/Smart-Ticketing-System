using System;
using System.Collections.Generic;

namespace SmartTicketingSystem.Models.ViewModels
{
    public class MyTicketsVM
    {
        public List<TicketGroupVM> UpcomingTickets { get; set; } = new List<TicketGroupVM>();
        public List<TicketGroupVM> PastTickets { get; set; } = new List<TicketGroupVM>();
        public TicketStatsVM Stats { get; set; } = new TicketStatsVM();
    }

    public class TicketGroupVM
    {
        public int BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string EventTitle { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string EventVenue { get; set; } = string.Empty;
        public List<TicketVM> Tickets { get; set; } = new List<TicketVM>();
    }

    public class TicketVM
    {
        public int TicketId { get; set; }
        public string QRCodeValue { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
    }

    public class TicketStatsVM
    {
        public int TotalTickets { get; set; }
        public int UpcomingEvents { get; set; }
        public int PastEvents { get; set; }
    }
}