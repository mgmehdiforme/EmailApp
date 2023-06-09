﻿using System.ComponentModel.DataAnnotations;

namespace EmailApp.ViewModels
{
    public class SendEmailViewModel
    {
        public int ConfigId { get; set; }

        [Required]
        public string To { get; set; }

        public string? Cc { get; set; }

        public string? Bcc { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }

        public IFormFile[]? Attachments { get; set; }
        public DateTime? CreateOn { get; set; }
        public int? CurrentPageNumber { get; set; }
    }
}
