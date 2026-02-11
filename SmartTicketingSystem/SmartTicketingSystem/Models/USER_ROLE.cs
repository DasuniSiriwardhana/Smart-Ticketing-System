using System.ComponentModel.DataAnnotations;

namespace SmartTicketingSystem.Models
{
    public class USER_ROLE
    {
        [Key]
        public int UserRoleID { get; set; }
        public int roleID { get; set; }
        public int userID { get; set; }
        public DateTime AssignedAt { get; set; }
        public int member_id { get; set; }

    }
}
