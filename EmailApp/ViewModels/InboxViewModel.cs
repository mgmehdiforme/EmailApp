using EmailApp.DataModels;

namespace EmailApp.ViewModels
{
    public class InboxViewModel
    {
        public List<EmailMessage> EmailMessages { get; set; }=new List<EmailMessage>();
        public int CurrentPageNumber { get; set; } = 1;
        public bool SyncedWithInbox { get; set; } = false;
        public string SyncError { get; set; } = "";
    }
}
