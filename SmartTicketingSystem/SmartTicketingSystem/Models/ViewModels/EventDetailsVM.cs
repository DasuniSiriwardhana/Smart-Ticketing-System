using System.Collections.Generic;
using SmartTicketingSystem.Models;

namespace SmartTicketingSystem.Models.ViewModels
{
    public class EventDetailsVM
    {
        public EVENT Event { get; set; }
        public List<TICKET_TYPE> TicketTypes { get; set; } = new();
        public Dictionary<int, int> Quantities { get; set; } = new(); // TicketID -> Qty
        public Dictionary<int, int> RemainingByTicketType { get; set; } = new(); // TicketID -> Remaining seats
        public int RemainingEventCapacity { get; set; }
    }
}
