using System.ComponentModel.DataAnnotations;

namespace SmartTicketingSystem.Models
{
    public class USER
    {
        [Key]
        public int member_id { get; set; }
        [Required]
        public string FullName { get; set; }
        public string Email { get; set; }
        public string phone { get; set; }
        public string passwordHash { get; set; }
        public string userType { get; set; }
        public string UniversityNumber { get; set; }
        public char isverified { get; set; }
        public string status { get; set; }
        public DateTime createdAt { get; set; }
        public int ApprovalID { get; set; }

    }
}
