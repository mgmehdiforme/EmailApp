namespace EmailApp.JsonModels
{
    public class MimeMessageMetaModel
    {

        // The list of addresses in the From header
        public List<string> From { get; set; }

        // The list of addresses in the To header
        public List<string> To { get; set; }

        // The list of addresses in the Cc header
        public List<string> Cc { get; set; }

        // The list of addresses in the Bcc header
        public List<string> Bcc { get; set; }

        // The list of addresses in the Reply-To header
        public List<string> ReplyTo { get; set; }
        public string BodyHtml { get; set; }
    }
}
