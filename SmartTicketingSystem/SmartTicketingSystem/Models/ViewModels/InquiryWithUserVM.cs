namespace SmartTicketingSystem.Models.ViewModels
{
    public class InquiryWithUserVM
    {
        // Inquiry properties
        public int InquiryID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string category { get; set; } = string.Empty;
        public string message { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public DateTime createdAt { get; set; }
        public int? HandleByUserID { get; set; }
        public DateTime? HandleAt { get; set; }
        public string? ResponseNote { get; set; }

        // Handler details (from JOIN with USER table)
        public string HandlerName { get; set; } = string.Empty;
        public string HandlerEmail { get; set; } = string.Empty;
    }
}