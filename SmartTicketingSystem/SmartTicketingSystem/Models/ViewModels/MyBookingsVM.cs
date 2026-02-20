namespace SmartTicketingSystem.Models.ViewModels
{
    public class MyBookingsVM
    {
        public List<BookingCardVM> Bookings { get; set; } = new();
        public BookingStatsVM Stats { get; set; } = new();
    }

    public class BookingCardVM
    {
        public int BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string EventTitle { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string EventVenue { get; set; } = string.Empty;
        public int TicketCount { get; set; }
        public decimal TotalAmount { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }

        public bool CanPay => PaymentStatus != "Paid";
        public bool CanCancel => BookingStatus != "Cancelled" && BookingStatus != "Completed";

        public string StatusColor => BookingStatus switch
        {
            "Confirmed" => "success",
            "PendingPayment" => "warning",
            "Cancelled" => "danger",
            "Completed" => "info",
            _ => "secondary"
        };

        public string PaymentStatusColor => PaymentStatus switch
        {
            "Paid" => "success",
            "Unpaid" => "danger",
            "Partial" => "warning",
            _ => "secondary"
        };
    }

    public class BookingStatsVM
    {
        public int TotalBookings { get; set; }
        public int UpcomingEvents { get; set; }
        public int PendingPayments { get; set; }
        public int CompletedEvents { get; set; }
        public decimal TotalSpent { get; set; }
        public int TotalTickets { get; set; }
    }
}