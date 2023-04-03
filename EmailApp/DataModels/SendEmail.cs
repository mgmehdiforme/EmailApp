using System.ComponentModel.DataAnnotations;

namespace EmailApp.DataModels
{
    public class SendEmail
    {
        [Key]
        public int Id { get; set; }
        public int ConfigId { get; set; }
       
        public string To { get; set; }

        public string? Cc { get; set; }

        public string? Bcc { get; set; }
        
        public string Subject { get; set; }
        
        public string Body { get; set; }

        public DateTime CreateOn { get; set; }
        public bool IsDraft { get; set; }=false;
    }
}
