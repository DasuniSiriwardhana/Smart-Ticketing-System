using System.ComponentModel.DataAnnotations;

namespace SmartTicketingSystem.Models
{
    public class EVENT_CATEGORY
    {
        [Key]
        public int categoryID { get; set; }
        public string categoryName { get; set; }
        public DateTime createdAt { get; set; }

    }
}
