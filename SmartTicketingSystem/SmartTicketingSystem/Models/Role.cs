using System.ComponentModel.DataAnnotations;

namespace SmartTicketingSystem.Models
{
    public class Role
    {
        [Key]

        public int RoleId { get; set; }
        [Required]
        [StringLength(100)]
        public string rolename { get; set; }
        public DateTime createdAt { get; set; }
    }
}
