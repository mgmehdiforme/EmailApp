using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using MailKit;
using EmailApp.DataModels;
using EmailApp.ViewModels;
using EmailApp.Data;
using EmailApp.JsonModels;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace EmailApp.Services
{
    public interface IEmailService
    {                
        InboxViewModel GetInbox(int pageNumber, int ConfigId);
        EmailMessageViewModel GetMessage(int messageId);
        AttachmentViewModel GetAttachment(int messageId, string attachmentKey);
    }

    public class EmailService:IEmailService
    {
        private readonly EmailAppContext _context;

        public EmailService(EmailAppContext context)
        {
            _context = context;
        }
        #region Email Server 
        /// <summary>
        /// Read Inbox Messages from server
        /// </summary>
        /// <param name="config">where to connect</param>
        /// <param name="searchQuery">what read default is new messages </param>
        /// <returns>list of message</returns>
        private List<MimeMessage> ReadMessagesImap(EmailConfig config, SearchQuery searchQuery=null)
        {
            if (searchQuery == null)
                searchQuery = SearchQuery.New;

            // connect to IMAP server
            var client = new ImapClient();
            ConnectClient(config, ref client);

            // select inbox folder
            client.Inbox.Open(FolderAccess.ReadOnly);

            // search for unread messages
            var unread = client.Inbox.Search(searchQuery);

            // get first 10 messages

            List<MimeKit.MimeMessage> messages = new List<MimeKit.MimeMessage>();
            foreach (var item in unread)
            {
                messages.Add(client.Inbox.GetMessage(item));
            }

            // disconnect from IMAP server
            client.Disconnect(true);
            client.Dispose();
            return messages;
        }

        /// <summary>
        /// read a message by messageId from Server
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="config"></param>
        /// <returns>return MimeMessage object</returns>
        private MimeMessage GetEmailFromServer(string messageId, EmailConfig config)
        {            
            // create an imap client
            var client = new ImapClient();
            ConnectClient(config, ref client);

            // get the inbox folder
            var inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadOnly);

            // search for the message by id
            var query = SearchQuery.HeaderContains("Message-Id", messageId);
            var uids = inbox.Search(query);

            // get the first matching message
            if (uids.Count > 0)
            {
                var uid = uids[0];
                var message = inbox.GetMessage(uid);

                client.Disconnect(true);
                client.Dispose();
                return message;
            }

            // disconnect from the imap server
            client.Disconnect(true);
            client.Dispose();

            // return null if no message found
            return null;
        }

        private (Stream?,string,string) GetAttachmentFromServer(string messageId, string attachmentContentId, EmailConfig config)
        {
            // create an imap client
            var client = new ImapClient();
            ConnectClient(config, ref client);

            // get the inbox folder
            var inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadOnly);

            // search for the message by id
            var query = SearchQuery.HeaderContains("Message-Id", messageId);
            var uids = inbox.Search(query);

            // get the first matching message
            if (uids != null && uids.Any())
            {
                var uid = uids[0];
                var message = inbox.GetMessage(uid);

                if (message != null)
                {
                    // Find the attachment by content id
                    MimePart attachment = null;
                    var contentTypeName = "";
                    var fileName = "";
                    message.Attachments.ToList().ForEach(x =>
                        {
                            if (x is MimePart part) 
                            {
                                if (part.ContentId==attachmentContentId || part.FileName==attachmentContentId)
                                {
                                    attachment = part;
                                    contentTypeName = x.ContentType?.Name;
                                    fileName = x.ContentDisposition?.FileName;
                                }                                
                            }
                        });

                    if (attachment != null)
                    {
                        // Get the attachment content stream
                        var stream = GetAttachmentContentStream(attachment);

                        client.Disconnect(true);
                        client.Dispose();
                        return (stream,contentTypeName,fileName);
                    }
                }
            }

            client.Disconnect(true);
            client.Dispose();
            return (null, "", "");
        }
        #endregion

        #region Return View Models
        public InboxViewModel GetInbox(int pageNumber, int ConfigId)
        {
            var result = new InboxViewModel();

            var searchQuery = SearchQuery.New;
            if (_context.EmailMessage.Count() == 0)
                searchQuery = SearchQuery.All;

            List<MimeKit.MimeMessage> serverMessages = new List<MimeKit.MimeMessage>();
            var config = _context.EmailConfig.Find(ConfigId);
            try
            {
                serverMessages = ReadMessagesImap(config, searchQuery);
                result.Synced = true;
            }
            catch (Exception ex)
            {
                result.Synced = false;
                result.SyncError = "Your Mail Box not Sync : " + ex.Message;
            }

            serverMessages.ForEach(x =>
            {
                _context.EmailMessage.Add(new EmailMessage
                {
                    Date = x.Date,
                    EmailConfigId = config.Id,
                    Flagged = x.Headers["Flagged"],
                    Label = x.Headers["X-Label"],
                    Priority = x.Priority.ToString(),
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

            var pageSize = 10;
            var startIndex = pageSize * (pageNumber - 1);
            result.TotalPageCount = _context.EmailMessage.Count() / pageSize;

            result.ConfigId = ConfigId;

            result.EmailMessages = _context.EmailMessage.Skip(startIndex).Take(pageSize).ToList();
            result.CurrentPageNumber = pageNumber;
            return result;
        }

        public EmailMessageViewModel GetMessage(int messageId)
        {
            var result = new EmailMessageViewModel();
            var dbMessage = _context.EmailMessage.Include(x=>x.EmailConfig).First(x => x.Id == messageId);
            result.Id=dbMessage.Id;
            MimeMessage message = null;
            try
            {
                message = GetEmailFromServer(dbMessage.MessageId, dbMessage.EmailConfig);
                result.Synced = true;
            }
            catch (Exception ex)
            {
                result.Synced = false;
                result.SyncError = ex.Message;
            }

            var attachments = new Dictionary<string, string>();
            if (message != null)
            {
                // iterate through the attachments
                foreach (var attachment in message.Attachments.Where(x=>x.IsAttachment))
                {
                    // check if the attachment is a mime part
                    if (attachment is MimePart part)
                    {
                        // get the file name from the part
                        var fileName = part.FileName;

                        // get the id from the part
                        var fileId = part.ContentId ?? part.FileName;

                        // add the file name to the list
                        attachments.Add(fileId, fileName);
                    }
                }
            }
            var messageMetadata = Newtonsoft.Json.JsonConvert.DeserializeObject<MimeMessageMetaModel>(dbMessage.JsonMessageMetadata);

            result.BodyHtml = messageMetadata.BodyHtml;
            result.From = messageMetadata.From.First();
            result.Label = dbMessage.Label;
            result.Priority = dbMessage.Priority;
            result.Subject = dbMessage.Subject;

            if (message != null)
            {

                result.Attachments = attachments;
                result.BodyHtml = message.HtmlBody;
                result.From = message.From.First().Name;
                result.Label = dbMessage.Label;
                result.Priority = dbMessage.Priority;
                result.Subject = message.Subject;

            }

            return result;
        }

        public AttachmentViewModel GetAttachment(int messageId,string attachmentKey)
        {
            var result = new AttachmentViewModel();
            var dbMessage = _context.EmailMessage.Include(x => x.EmailConfig).First(x => x.Id == messageId);
            try
            {
                var serverResult = GetAttachmentFromServer(dbMessage.MessageId, attachmentKey, dbMessage.EmailConfig);
                result.FileName = serverResult.Item3;
                result.ContentType = serverResult.Item2;
                result.FileStream = serverResult.Item1;
            }
            catch (Exception ex)
            {
                return null;
            }
            return result;
        }
        #endregion

        #region private Helpers
        private Stream GetAttachmentContentStream(MimePart attachment)
        {
            var memoryStream = new MemoryStream();
            attachment.Content.DecodeTo(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }

        private void ConnectClient(EmailConfig config, ref ImapClient iMapClient)
        {
            iMapClient.Connect(config.ImapServer, //"imap.gmail.com"
             config.ImapPort,// 993
             config.UseSSLForImap ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.Auto);
            iMapClient.Authenticate(
                            config.UserName,//"your_email@gmail.com"
                            config.Password// "your_password"
                            );
        }
        #endregion

    }
}
