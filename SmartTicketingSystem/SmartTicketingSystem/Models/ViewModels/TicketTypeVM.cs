namespace SmartTicketingSystem.Models.ViewModels
{
    public class TicketTypeVM
    {
        public int TicketTypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int AvailableQuantity { get; set; }
        public int SeatLimit { get; set; }
    }
}