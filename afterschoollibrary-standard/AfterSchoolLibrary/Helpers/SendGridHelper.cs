using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using SendGrid;
using SendGrid.Helpers.Mail;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchoolLibrary
{
    public class SendGridHelper
    {
        public const string EnrollHQGenericTemplateId = "c672e668-1b54-4721-84be-50fe3cde7cb2";
        public const string AfterSchoolHQGenericTemplateId = "70362a81-6361-4140-9d5b-25b28e97ceca";
        public const string MarketingUrlPath = "marketing/contacts";
        public static string ProductionSendGridApiKey => "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";

        public static string DevelopmentSendGridApiKey =>
            "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";

        public static string SendGridApiKey
        {
            get
            {
                if (Utilities.IsProduction)
                    return ProductionSendGridApiKey;

                return DevelopmentSendGridApiKey;
            }
        }

        public static string GetGenericTemplateId(bool isAfterSchool = true)
        {
            if (isAfterSchool)
                return AfterSchoolHQGenericTemplateId;

            return EnrollHQGenericTemplateId;
        }

        public static SystemMessages SendMail(string fromEmail, string toEmail, string subject, string body,
            string replyTo = null, string emailTemplateId = null)
        {
            EmailAddress replyToEmail = null;
            if (replyTo.IsNotBlank())
                replyToEmail = new EmailAddress(replyTo);

            return SendMail(new EmailAddress(fromEmail), new EmailAddress(toEmail), subject, body, replyToEmail,
                emailTemplateId);
        }

        public static SystemMessages SendMail(EmailAddress fromEmail, EmailAddress toEmail, string subject, string body,
            EmailAddress replyTo = null, string emailTemplateId = null, EmailAddress ccEmail = null)
        {
            var msgs = new SystemMessages();
            /*if (Utilities.IsLocalHost)
            {
                msgs.AddRange(SMTPMailTrapHelper.SendMail(
                fromEmail,
                toEmail,
                subject,
                body,
                msgs,
                emailTemplateId: emailTemplateId
                ));
            }
            else
            {*/
            var sg = new SendGridClient(SendGridApiKey);

            // if (!Utilities.IsProduction)
            //     subject = $"{Utilities.Environment} ENV: {subject}";

            var mail = MailHelper.CreateSingleEmail(fromEmail, toEmail, subject, null, body);

            mail.ReplyTo = replyTo;
            mail.TemplateId = emailTemplateId ?? EnrollHQGenericTemplateId;
            if (ccEmail != null)
                mail.AddCc(ccEmail);

            var task = Task.Run(async () => await sg.SendEmailAsync(mail));
            task.Wait();

            dynamic response = task.Result;
            if (response.StatusCode != HttpStatusCode.Accepted)
                msgs.Add(response.Body.ReadAsStringAsync().Result, MessageType.Error);
            //}
            return msgs;
        }

        public static SystemMessages SendMailWithAttachments(EmailAddress fromEmail, EmailAddress toEmail,
            string subject, string body, string emailTemplateId, byte[] attachmentByteData, string attachmentFilename)
        {
            var msgs = new SystemMessages();
            if (Utilities.IsLocalHost)
                msgs.AddRange(SMTPMailTrapHelper.SendMailwithAttachment(fromEmail,
                    toEmail,
                    subject,
                    body,
                    attachmentByteData,
                    attachmentFilename,
                    msgs));
            else
            {
                var sg = new SendGridClient(SendGridApiKey);

                if (!Utilities.IsProduction)
                    subject = $"{Utilities.Environment} ENV: {subject}";

                var mail = MailHelper.CreateSingleEmail(fromEmail, toEmail, subject, null, body);

                if (attachmentByteData != null)
                    mail.Attachments = new List<Attachment>
                    {
                        new Attachment
                        {
                            Content = Convert.ToBase64String(attachmentByteData),
                            Filename = attachmentFilename ?? "registrations.csv",
                            Type = "text/csv",
                            Disposition = "attachment",
                        },
                    };

                mail.TemplateId = emailTemplateId ?? EnrollHQGenericTemplateId;

                var task = Task.Run(async () => await sg.SendEmailAsync(mail));
                task.Wait();

                dynamic response = task.Result;
                if (response.StatusCode != HttpStatusCode.Accepted)
                    msgs.Add(response.Body.ReadAsStringAsync().Result, MessageType.Error);
            }
            return msgs;
        }
    }
}