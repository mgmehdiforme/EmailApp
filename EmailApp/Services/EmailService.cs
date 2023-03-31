using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using MailKit;
using EmailApp.DataModels;
using MailKit.Net.Pop3;

namespace EmailApp.Services
{
    public interface IEmailService
    {
        List<MimeKit.MimeMessage> ReadMessagesPop3(EmailConfig config, SearchQuery searchQuery = null, int maxCount = 10);
        List<MimeKit.MimeMessage> ReadMessagesImap(EmailConfig config, SearchQuery searchQuery = null);
    }

    public class EmailService:IEmailService
    {
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

            // ... load or create the message
            //var label = messages[0].Headers["X-Label"]; // get the value of the 
            //var priority = messages[0].Headers["X-Priority"]; // get 
            //var flagged = messages[0].Headers["Flagged"]; // get 

            // disconnect from IMAP server
            imap.Disconnect(true);
            return messages;
        }

        /// <summary>
        /// Read Messages from Inbox
        /// </summary>
        /// <param name="config">where to connect</param>
        /// <param name="searchQuery">what read default is new messages </param>
        /// <returns>list of message</returns>
        public List<MimeKit.MimeMessage> ReadMessagesPop3(EmailConfig config, SearchQuery searchQuery = null, int maxCount = 10)
        {
            if (searchQuery == null)
                searchQuery = SearchQuery.New;

            // Create a new Pop3Client instance
            using (var emailClient = new Pop3Client())
            {
                // Connect to the POP3 server using SSL
                emailClient.Connect(config.Pop3Server, config.Pop3Port, config.UseSSLForPop3);
                // Remove any authentication mechanisms that require OAuth 2.0
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                // Authenticate with the POP3 server
                emailClient.Authenticate(config.UserName, config.Password);

                // Create a list to store the email messages
                List<MimeKit.MimeMessage> emails = new List<MimeKit.MimeMessage>();

                // Loop through the messages on the server up to the maxCount
                for (int i = 0; i < emailClient.Count && i < maxCount; i++)
                {
                    // Get the message from the server
                    var message = emailClient.GetMessage(i);
                    // Add the sender and recipient addresses
                    //emailMessage.FromAddresses.AddRange(message.From.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));
                    //emailMessage.ToAddresses.AddRange(message.To.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));
                    // Add the email message to the list
                    emails.Add(message);
                }
                return emails;
            }
        }
    }
}
