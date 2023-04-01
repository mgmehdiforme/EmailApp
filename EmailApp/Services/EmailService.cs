using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using MailKit;
using EmailApp.DataModels;
using MailKit.Net.Pop3;
using EmailApp.ViewModels;
using EmailApp.Data;
using EmailApp.JsonModels;
using System.Drawing.Printing;
using System.Net.Mail;

namespace EmailApp.Services
{
    public interface IEmailService
    {        
        List<MimeKit.MimeMessage> ReadMessagesImap(EmailConfig config, SearchQuery searchQuery = null);
        InboxViewModel GetInbox(int pageNumber, int ConfigId);
    }

    public class EmailService:IEmailService
    {
        private readonly EmailAppContext _context;

        public EmailService(EmailAppContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Read Messages from Inbox
        /// </summary>
        /// <param name="config">where to connect</param>
        /// <param name="searchQuery">what read default is new messages </param>
        /// <returns>list of message</returns>
        public List<MimeKit.MimeMessage> ReadMessagesImap(EmailConfig config, SearchQuery searchQuery=null)
        {
            if (searchQuery == null)
                searchQuery = SearchQuery.New;

            // connect to IMAP server
            using var imap = new ImapClient();
            imap.Connect(config.ImapServer, //"imap.gmail.com"
                         config.ImapPort,// 993
                         config.UseSSLForImap ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.Auto);
            imap.Authenticate(
                            config.UserName,//"your_email@gmail.com"
                            config.Password// "your_password"
                            );

            // select inbox folder
            imap.Inbox.Open(FolderAccess.ReadOnly);

            // search for unread messages
            var unread = imap.Inbox.Search(searchQuery);

            // get first 10 messages

            List<MimeKit.MimeMessage> messages = new List<MimeKit.MimeMessage>();
            foreach (var item in unread)
            {
                messages.Add(imap.Inbox.GetMessage(item));
            }

            // disconnect from IMAP server
            imap.Disconnect(true);
            return messages;
        }

        public InboxViewModel GetInbox(int pageNumber,int ConfigId)
        {
            var result=new InboxViewModel();

            var searchQuery = SearchQuery.New;
            if (_context.EmailMessage.Count() == 0)
                searchQuery = SearchQuery.All;

            List<MimeKit.MimeMessage> serverMessages = new List<MimeKit.MimeMessage>();
            var config = _context.EmailConfig.Find(ConfigId);
            try
            {
                serverMessages= ReadMessagesImap(config, searchQuery);
            }
            catch (Exception ex)
            {
                result.SyncError ="Your Mail Box not Sync : "+ex.Message;
            }
            //var label = messages[0].Headers["X-Label"]; // get the value of the 
            //var priority = messages[0].Headers["X-Priority"]; // get 
            //var flagged = messages[0].Headers["Flagged"]; // get 
            serverMessages.ForEach(x =>
            {
                _context.EmailMessage.Add(new EmailMessage
                {
                    Date = x.Date,
                    EmailConfigId = config.Id,
                    Flagged = x.Headers["Flagged"],
                    Label = x.Headers["X-Label"],
                    Priority = x.Headers["X-Priority"],
                    MessageId = x.MessageId,
                    From = x.From.First().Name,
                    Subject = x.Subject,
                    JsonMessageMetadata = Newtonsoft.Json.JsonConvert.SerializeObject(new MimeMessageMetaModel()
                    {
                        Bcc = x.Bcc.Select(b => b.Name).ToList(),
                        Cc = x.Cc.Select(c => c.Name).ToList(),
                        From = x.From.Select(f => f.Name).ToList(),
                        ReplyTo = x.ReplyTo.Select(r => r.Name).ToList(),
                        To = x.To.Select(x => x.Name).ToList(),
                        BodyHtml = x.HtmlBody,
                    })
                });
            });
            _context.SaveChanges();
            var pageSize = 15;
            var startIndex = pageSize * (pageNumber - 1);
            var endIndex = startIndex + pageSize - 1;
            var totalCount = _context.EmailMessage.Count();

            if (endIndex >= totalCount)
            {
                endIndex = totalCount - 1;
            }
            
            result.EmailMessages= _context.EmailMessage.Skip(startIndex).Take(endIndex).ToList();
            result.CurrentPageNumber = pageNumber;
            return result;
        }
    }
}
