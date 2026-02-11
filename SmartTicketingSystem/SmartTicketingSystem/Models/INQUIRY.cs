using System.ComponentModel.DataAnnotations;

namespace SmartTicketingSystem.Models
{
    public class INQUIRY
    {
        [Key]
        public int InquiryID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string category { get; set; }
        public string message { get; set; }
        public string status { get; set; }
        public DateTime createdAt { get; set; }
        public int HandleByUserID { get; set; }
        public DateTime HandleAt {  get; set; }
        public string ResponseNote { get; set; }

      
       
    }
}
