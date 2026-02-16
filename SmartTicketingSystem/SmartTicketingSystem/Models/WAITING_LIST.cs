using System.ComponentModel.DataAnnotations;

namespace SmartTicketingSystem.Models
{
    public class WAITING_LIST
    {
        [Key]
        public int WaitingListID { get; set; }
        public int EventID { get; set; }
        public int member_id { get; set; }
        public DateTime AddedAt { get; set; }
        public string Status { get; set; }

    }
}
