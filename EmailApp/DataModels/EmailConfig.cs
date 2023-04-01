using System.ComponentModel.DataAnnotations;

namespace EmailApp.DataModels
{
    public class EmailConfig
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; } 
        public string From { get; set; }
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public bool UseSSLForSmtp { get; set; }
        public string ImapServer { get; set; }
        public int ImapPort { get; set; }
        public bool UseSSLForImap { get; set; }
        public bool UseImap { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        
    }
}
