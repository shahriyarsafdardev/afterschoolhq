using BasicLibrary;
using MailKit.Net.Smtp;
using MimeKit;
using SendGrid.Helpers.Mail;
using System;
using System.Configuration;
using System.IO;

namespace AfterSchoolLibrary.Helpers
{
    public class SMTPMailTrapHelper
    {
        public SMTPMailTrapHelper(string username, string password, string host, string port)
        {
            MailTrapUsername = username;
            MailTrapPassword = password;
            MailTrapHost = host;
            MailTrapPort = port;
        }

        public static string MailTrapUsername { get; set; }
        public static string MailTrapPassword { get; set; }
        public static string MailTrapHost { get; set; }
        public static string MailTrapPort { get; set; }
        public static SystemMessages SendMail(EmailAddress fromEmail, EmailAddress toEmail, string subject, string body, SystemMessages msgs,
         EmailAddress replyTo = null, EmailAddress ccEmail = null)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromEmail.Name, fromEmail.Email));
                message.To.Add(new MailboxAddress(toEmail.Name, toEmail.Email));
                message.Subject = subject;
                message.Body = new TextPart("plain") { Text = body };

                if (replyTo != null && !string.IsNullOrEmpty(replyTo.ToString()))
                    message.ReplyTo.Add(new MailboxAddress("", replyTo.ToString()));

                if (ccEmail != null && !string.IsNullOrEmpty(ccEmail.ToString()))
                    message.Cc.Add(new MailboxAddress("", ccEmail.ToString()));

                using (var client = new SmtpClient())
                {
                    client.Connect(MailTrapHost, Convert.ToInt32(MailTrapPort));
                    client.Authenticate(MailTrapUsername, MailTrapPassword);
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                Utilities.SystemErrorLog(ex);
                msgs.Add(ex.Message, MessageType.Error, ex);
            }

            return msgs;
        }
        public static SystemMessages SendMailwithAttachment(EmailAddress fromEmail, EmailAddress toEmail, string subject, string body,
            byte[] attachmentData, string attachmentFilename, SystemMessages msgs)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(fromEmail.Name, fromEmail.Email));
                message.To.Add(new MailboxAddress(toEmail.Name, toEmail.Email));
                message.Subject = subject;
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.TextBody = body;
                if (attachmentData.IsNotEmpty())
                {
                    var attachment = new MimePart("application", "octet-stream")
                    {
                        Content = new MimeContent(new MemoryStream(attachmentData), ContentEncoding.Default),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = attachmentFilename
                    };
                    bodyBuilder.Attachments.Add(attachment);
                }
                message.Body = bodyBuilder.ToMessageBody();
                using (var client = new SmtpClient())
                {
                    client.Connect(MailTrapHost, Convert.ToInt32(MailTrapPort));
                    client.Authenticate(MailTrapUsername, MailTrapPassword);
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                Utilities.SystemErrorLog(ex);
                msgs.Add(ex.Message, MessageType.Error, ex);
            }
            return msgs;
        }
    }
}
