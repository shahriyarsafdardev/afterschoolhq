using MailKit.Net.Smtp;
using MimeKit;
using SendGrid.Helpers.Mail;
using System;
using System.Configuration;

namespace AfterSchoolLibrary.Helpers
{
    public class SMTPMailTrapHelper
    {
        public static string MailTrapUsername => ConfigurationManager.AppSettings["MailTrap.Username"];
        public static string MailTrapPassword => ConfigurationManager.AppSettings["MailTrap.Password"];
        public static string MailTrapHost => ConfigurationManager.AppSettings["MailTrap.Host"];
        public static string MailTrapPort => ConfigurationManager.AppSettings["MailTrap.Port"];

        public static bool SendMail(EmailAddress fromEmail, EmailAddress toEmail, string subject, string body,
         EmailAddress replyTo = null, string emailTemplateId = null, EmailAddress ccEmail = null)
        {
            bool response = false;
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("", fromEmail.ToString()));
                message.To.Add(new MailboxAddress("", toEmail.ToString()));
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

                    response = true;
                }
            }
            catch (Exception)
            {
                response = false;
            }
            return response;
        }
    }
}
