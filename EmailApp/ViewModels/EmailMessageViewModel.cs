namespace EmailApp.ViewModels
{
    public class EmailMessageViewModel
    {
        public int Id { get; set; }
        public int CurrentPageNuber { get; set; }
        public int ConfigId { get; set; }
        public string Subject { get; set; } = "";
        public string BodyHtml { get; set; } = "";
        public string From { get; set; } = "";
        public string Label { get; set; } = "";
        public string Priority { get; set; } = "";
        public bool Synced { get; set; } = false;
        public string SyncError { get; set; } = "";
        public bool IsArchived { get; set; } = false;
        public DateTimeOffset Date { get; set; }
        public Dictionary<string,string> Attachments { get; set; }= new Dictionary<string,string>();
    }
}
