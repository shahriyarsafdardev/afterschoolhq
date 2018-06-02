using BasicLibrary;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AfterSchoolLibrary
{
    public class SendGridHelper
    {
        public static string SendGridApiKey { get { return "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"; } }
        public static string EnrollHQGenericTemplateId { get { return "c672e668-1b54-4721-84be-50fe3cde7cb2"; } }
        public static string AfterSchoolHQGenericTemplateId { get { return "70362a81-6361-4140-9d5b-25b28e97ceca"; } }

        public static string GetGenericTemplateId(bool isAfterSchool)
        {
            if (isAfterSchool)
                return AfterSchoolHQGenericTemplateId;
            return EnrollHQGenericTemplateId;
        }

        public static SystemMessages SendMail(string fromEmail, string toEmail, string subject, string body, string replyTo = null, string emailTemplateId = null)
        {
            EmailAddress replyToEmail = null;
            if (replyTo.IsNotBlank())
            {
                replyToEmail = new EmailAddress(replyTo);
            }

            return SendMail(new EmailAddress(fromEmail), new EmailAddress(toEmail), subject, body, replyToEmail, emailTemplateId);
        }

        public static SystemMessages SendMail(EmailAddress fromEmail, EmailAddress toEmail, string subject, string body, EmailAddress replyTo = null, string emailTemplateId = null)
        {
            var msgs = new SystemMessages();
            
            var sg = new SendGridClient(SendGridApiKey);
            // var content = new Content("text/html", body);

            var mail = MailHelper.CreateSingleEmail(fromEmail, toEmail, subject, null, body);
            mail.ReplyTo = replyTo;
            mail.TemplateId = emailTemplateId ?? EnrollHQGenericTemplateId;
            
            var task = System.Threading.Tasks.Task.Run(async () =>
            {
                return await sg.SendEmailAsync(mail);
            });
            task.Wait();

            dynamic response = task.Result;
            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                msgs.Add(response.Body.ReadAsStringAsync().Result, MessageType.Error);
            }

            return msgs;
        }
    }
}
