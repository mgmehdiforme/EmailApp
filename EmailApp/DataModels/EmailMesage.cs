using MimeKit;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmailApp.DataModels
{
    public class EmailMessage
    {
        [Key]
        public int Id { get; set; }

        // The message identifier
        public string? MessageId { get; set; }

        // The date of the message
        public DateTimeOffset Date { get; set; }

        // The subject of the message
        public string? Subject { get; set; }

        // The from of the message
        public string? From { get; set; }

        // the value of the X-Label header
        public string? Label { get; set; }

        // the value of the X-Priority header
        public string? Priority { get; set; }

        // the value of the Flagged header
        public string? Flagged { get; set; }

        [ForeignKey(nameof(EmailConfig))]
        public int EmailConfigId { get; set; }

        public string? JsonMessageMetadata { get; set; }
        public virtual EmailConfig  EmailConfig{ get; set; }
    }



}
