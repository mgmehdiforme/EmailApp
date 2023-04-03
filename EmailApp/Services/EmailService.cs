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
using Microsoft.VisualBasic;
using System.Collections.Generic;

namespace EmailApp.Services
{
    public interface IEmailService
    {
        /// <summary>
        /// Retrieves a page of email messages for a given email configuration and page number, and returns an InboxViewModel object.
        /// </summary>
        /// <param name="pageNumber">An integer representing the page number to retrieve. Must be greater than zero.</param>
        /// <param name="ConfigId">An integer representing the unique identifier of the email configuration to retrieve messages for.</param>
        /// <returns>An InboxViewModel object containing a page of email messages and metadata about the email inbox.</returns>
        /// <remarks>
        /// This function creates a new InboxViewModel object to hold the results of the operation.
        /// If the email message count in the database is zero, the function sets a searchQuery variable to All, indicating that all messages should be retrieved from the server.
        /// Otherwise, the searchQuery variable is set to New, indicating that only new messages should be retrieved.
        /// The function attempts to read email messages from the server using the specified email configuration and search query.
        /// If successful, the function populates the EmailMessage entity with message data and adds it to the database.
        /// The changes are saved to the database using the _context.SaveChanges() method.
        /// The function then retrieves a page of email messages from the database based on the specified page number and page size.
        /// Finally, the function populates the InboxViewModel object with the retrieved email messages and metadata, and returns it to the caller.
        /// </remarks>
        InboxViewModel GetInbox(int pageNumber, int ConfigId);

        /// <summary>
        /// Retrieves an email message with the given messageId from the database and the server, populates the result with metadata and attachments, and returns it as an EmailMessageViewModel.
        /// </summary>
        /// <param name="messageId">The ID of the email message to retrieve.</param>
        /// <returns>An EmailMessageViewModel object representing the email message.</returns>
        /// <remarks>
        /// This method retrieves an email message by its message id from the email server and returns an EmailMessageViewModel object that contains metadata and attachment information.
        /// It first retrieves the email message from the database using the provided messageId, and then attempts to retrieve the message from the email server.
        /// If the server retrieval succeeds, the method populates the EmailMessageViewModel object with message metadata and attachment information retrieved from the email server.
        /// If the server retrieval fails, the method sets the Synced flag in the EmailMessageViewModel object to false and sets the SyncError property to the error message returned by the email server.
        /// If the email message does not have any attachments, the Attachments dictionary in the EmailMessageViewModel object will be empty.
        /// </remarks>
        EmailMessageViewModel GetMessage(int messageId);

        /// <summary>
        /// Retrieves an email attachment for a given message and attachment key, and returns an AttachmentViewModel object.
        /// </summary>
        /// <param name="messageId">An integer representing the unique identifier of the email message containing the attachment.</param>
        /// <param name="attachmentKey">A string representing the unique identifier of the attachment to retrieve.</param>
        /// <returns>An AttachmentViewModel object containing information about the retrieved attachment, including its filename, content type, and stream.</returns>
        /// <remarks>
        /// This function creates a new AttachmentViewModel object to hold the results of the operation.
        /// The function retrieves the EmailMessage entity with the specified messageId from the database, and includes the associated EmailConfig entity in the query results.
        /// The function attempts to retrieve the specified attachment from the email server using the GetAttachmentFromServer method.
        /// If successful, the function populates the AttachmentViewModel object with information about the attachment, including its filename, content type, and stream.
        /// The function returns the AttachmentViewModel object to the caller.
        /// If the function encounters an exception while retrieving the attachment, it returns null.
        /// </remarks>
        AttachmentViewModel GetAttachment(int messageId, string attachmentKey);

        /// <summary>
        /// Archives an email message by updating its IsArchive property and returns the EmailConfigId of the updated entity.
        /// </summary>
        /// <param name="messageId">An integer representing the unique identifier of the email message to archive.</param>
        /// <returns>An integer representing the EmailConfigId of the updated email message entity.</returns>
        /// <remarks>
        /// This function retrieves an email message entity with the specified ID and its related EmailConfig entity using the _context object.
        /// The function sets the IsArchive property of the retrieved email message entity to true, indicating that the message has been archived.
        /// The changes are saved to the database using the _context.SaveChanges() method.
        /// </remarks>
        int SetMessageAsArchived(int messageId);

        /// <summary>
        /// Sets the specified email message as deleted by updating the "IsDeleted" flag in the database.
        /// </summary>
        /// <param name="messageId">The ID of the email message to mark as deleted.</param>
        /// <returns>The ID of the email configuration associated with the deleted message.</returns>
        /// <remarks>
        /// This method updates the "IsDeleted" flag of the specified email message to mark it as deleted in the database.
        /// It then saves the changes to the database using the Entity Framework context.
        /// The method returns the ID of the email configuration associated with the deleted message.
        /// </remarks>
        int SetMessageAsDeleted(int messageId);

        /// <summary>
        /// Sends an email using the given view model and email configuration ID.
        /// </summary>
        /// <param name="model">The SendEmailViewModel containing the email details.</param>
        /// <param name="configId">The ID of the email configuration to use for sending the email.</param>
        /// <remarks>
        /// The method retrieves the email configuration using the provided ID from the database.
        /// It then constructs a new MimeMessage object and sets the sender, recipients, subject, and body of the email.
        /// If the view model contains any attachments, they are added to the email as well.
        /// Finally, the email is sent using the SMTP settings from the email configuration.
        /// </remarks>
        int SendEmail(SendEmailViewModel model, int configId);
    }

    public class EmailService:IEmailService
    {
        private readonly EmailAppContext _context;
        private const int pageSize = 2;
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

        public int SendEmail(SendEmailViewModel model ,int configId)
        {
            var emailConfig=_context.EmailConfig.Find(configId);
            // Create a new MimeMessage
            var message = new MimeMessage();

            // Set the sender and recipients
            message.From.Add(new MailboxAddress(emailConfig.From, emailConfig.UserName));
            message.To.AddRange(GetAddresses(model.To));
            if (!string.IsNullOrEmpty(model.Cc))
            {
                message.Cc.AddRange(GetAddresses(model.Cc));
            }
            if (!string.IsNullOrEmpty(model.Bcc))
            {
                message.Bcc.AddRange(GetAddresses(model.Bcc));
            }

            // Set the subject and body
            message.Subject = model.Subject;
            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = model.Body;
            if (model.Attachments != null && model.Attachments.Length > 0)
            {
                foreach (var attachment in model.Attachments)
                {
                    using (var stream = attachment.OpenReadStream())
                    {
                        bodyBuilder.Attachments.Add(attachment.FileName, stream);
                    }
                }
            }
            message.Body = bodyBuilder.ToMessageBody();

            // Send the email

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                client.Connect(emailConfig.SmtpServer, emailConfig.SmtpPort, emailConfig.UseSSLForSmtp);
                client.Authenticate(emailConfig.UserName, emailConfig.Password);
                client.Send(message);
                var sendEmail = new DataModels.SendEmail
                {
                    Bcc = model.Cc,
                    Subject = model.Subject,
                    Body = model.Body,
                    To = model.To,
                    Cc = model.Cc,
                    ConfigId = model.ConfigId,
                    CreateOn = DateTime.Now
                };
                _context.SendEmail.Add(sendEmail);
                _context.SaveChanges();
                client.Disconnect(true);
                return sendEmail.Id;
            }
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

        #region Actions On Database
        public int SetMessageAsArchived(int messageId)
        {
            var dbMessage = _context.EmailMessage.Include(x => x.EmailConfig).First(x => x.Id == messageId);
            dbMessage.IsArchive = !dbMessage.IsArchive;

            _context.SaveChanges();
            return dbMessage.EmailConfigId;
        }

        public int SetMessageAsDeleted(int messageId)
        {
            var dbMessage = _context.EmailMessage.Include(x => x.EmailConfig).First(x => x.Id == messageId);
            dbMessage.IsDeleted = true;

            _context.SaveChanges();
            return dbMessage.EmailConfigId;
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
                if (!_context.EmailMessage.Any(y => y.MessageId == x.MessageId))
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
                }
            });
            _context.SaveChanges();

            
            var startIndex = pageSize * (pageNumber - 1);
            result.TotalPageCount = _context.EmailMessage.Count(x=>!x.IsDeleted) / pageSize;

            result.ConfigId = ConfigId;
            result.CurrentFolderName = "Inbox";
            result.EmailMessages = _context.EmailMessage.Where(x => !x.IsDeleted).Skip(startIndex).Take(pageSize).ToList();
            result.CurrentPageNumber = pageNumber;
            return result;
        }

        public EmailMessageViewModel GetMessage(int messageId)
        {
            var result = new EmailMessageViewModel();
            var dbMessage = _context.EmailMessage.Include(x=>x.EmailConfig).First(x => !x.IsDeleted && x.Id == messageId);
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
            result.IsArchived = dbMessage.IsArchive;
            result.ConfigId = dbMessage.EmailConfigId;
            result.Date = dbMessage.Date;
            if (message != null)
            {

                result.Attachments = attachments;
                result.BodyHtml = message.HtmlBody;
                result.From = message.From.First().Name;
                result.Label = dbMessage.Label;
                result.Priority = dbMessage.Priority;
                result.Subject = message.Subject;

            }
            dbMessage.IsRead = true;
            _context.SaveChanges();
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

        private IEnumerable<MailboxAddress> GetAddresses(string addressList)
        {
            var addresses = addressList.Split(',');
            foreach (var address in addresses)
            {
                yield return new MailboxAddress(address.Trim(), address.Trim());
            }
        }
        #endregion

    }
}
