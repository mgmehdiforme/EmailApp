using System.ComponentModel.DataAnnotations;

namespace EmailApp.ViewModels
{
    public class SendEmailViewModel
    {
        public int configId { get; set; }
        [Required]
        public string To { get; set; }

        public string? Cc { get; set; }

        public string? Bcc { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }

        public IFormFile[]? Attachments { get; set; }
    }
}
