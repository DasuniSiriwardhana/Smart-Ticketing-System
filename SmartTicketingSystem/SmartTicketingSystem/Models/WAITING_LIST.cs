using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTicketingSystem.Models
{
    public class WAITING_LIST
    {
        [Key]
        public int WaitingListID { get; set; }
        public int EventID { get; set; }
        public int member_id { get; set; }
        public DateTime AddedAt { get; set; }
        // Add default value
        public string Status { get; set; } = "Pending"; 

        // ===== ADD THESE NAVIGATION PROPERTIES =====
        [ForeignKey("EventID")]
        public virtual EVENT? Event { get; set; }

        [ForeignKey("member_id")]
        public virtual USER? User { get; set; }
    }
}