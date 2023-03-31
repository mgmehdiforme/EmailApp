using System.ComponentModel.DataAnnotations;

namespace EmailApp.Models
{
    public class EmailConfig
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; } 
        public string From { get; set; }
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
